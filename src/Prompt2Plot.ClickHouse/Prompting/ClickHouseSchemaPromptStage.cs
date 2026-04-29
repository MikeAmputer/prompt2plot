using System.Diagnostics;
using System.Text;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.ClickHouse.Logging;

namespace Prompt2Plot.ClickHouse;

/// <summary>
/// A prompt pipeline stage that appends ClickHouse database schema information
/// to the generated prompt.
/// </summary>
/// <remarks>
/// This stage queries ClickHouse system tables to discover database schema
/// metadata and formats it into a textual description suitable for inclusion
/// in a language model prompt.
///
/// The schema includes:
/// <list type="bullet">
/// <item><description>Databases and tables</description></item>
/// <item><description>Table engines</description></item>
/// <item><description>Columns and data types</description></item>
/// <item><description>Table and column comments (if available)</description></item>
/// </list>
///
/// The generated schema prompt is cached for the duration specified by <see cref="ClickHouseSchemaPromptStageSettings"/>.
/// </remarks>
public sealed class ClickHouseSchemaPromptStage : IPromptPipelineStage
{
	private readonly string _connectionString;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly string _httpClientName;

	private readonly string[] _includedDatabases;
	private readonly (string database, string table)[] _includedTables;
	private readonly (string database, string table)[] _excludedTables;
	private readonly string[] _excludedEngines;

	private volatile string _cachedPrompt = string.Empty;
	private readonly Stopwatch _cacheTimer = new();
	private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
	private readonly TimeSpan _cacheDuration;

	private readonly ILogger _logger;

	public ClickHouseSchemaPromptStage(
		ClickHouseSchemaPromptStageSettings settings,
		ILoggerFactory? loggerFactory = null)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(settings.ConnectionSettings);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionSettings.ConnectionString);
		ArgumentNullException.ThrowIfNull(settings.ConnectionSettings.HttpClientFactory);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionSettings.ConnectionString);

		_connectionString = settings.ConnectionSettings.ConnectionString;
		_httpClientFactory = settings.ConnectionSettings.HttpClientFactory;
		_httpClientName = settings.ConnectionSettings.HttpClientName;

		_includedDatabases = settings.IncludedDatabases.Length > 0
			? settings.IncludedDatabases
			: throw new InvalidOperationException("No ClickHouse databases included.");

		_includedTables = settings.IncludedTables;
		_excludedTables = settings.ExcludedTables;
		_excludedEngines = settings.ExcludedEngines;
		_cacheDuration = settings.CacheDuration;

		var logFactory = loggerFactory ?? NullLoggerFactory.Instance;
		_logger = logFactory.CreateLogger<ClickHouseSchemaPromptStage>();
	}

	public async Task ExecuteAsync(PromptContext context, CancellationToken cancellationToken)
	{
		if (!_cacheTimer.IsRunning || _cacheTimer.Elapsed >= _cacheDuration)
		{
			await UpdateCachedPrompt(cancellationToken);
		}

		if (!_cacheTimer.IsRunning || string.IsNullOrWhiteSpace(_cachedPrompt))
		{
			context.Errors.Add("Unable to fetch ClickHouse database schema.");

			return;
		}

		context.Prompt += _cachedPrompt;

		SchemaPromptLogs.SchemaPromptAppended(_logger, _cachedPrompt.Length, context.Prompt.Length, context.WorkItemId);
	}

	private async Task UpdateCachedPrompt(CancellationToken cancellationToken)
	{
		await _cacheSemaphore.WaitAsync(cancellationToken);
		try
		{
			if (_cacheTimer.IsRunning && _cacheTimer.Elapsed < _cacheDuration)
			{
				return;
			}

			SchemaPromptLogs.SchemaRefreshStarted(_logger);

			await using var clickHouseConnection = new ClickHouseConnection(
				_connectionString,
				_httpClientFactory,
				_httpClientName);

			var tables = await FetchTables(clickHouseConnection, cancellationToken);

			if (tables.Count == 0)
			{
				SchemaPromptLogs.NoTablesDiscovered(_logger);
			}

			await FetchColumns(clickHouseConnection, tables, cancellationToken);

			var sb = new StringBuilder();

			sb.AppendLine();
			sb.AppendLine("Database schema:");

			foreach (var tbl in tables.Values)
			{
				sb.Append(tbl.ToPromptString());
				sb.AppendLine();
			}

			sb.AppendLine(ClickHouseRules);

			_cachedPrompt = sb.ToString();
			_cacheTimer.Restart();

			SchemaPromptLogs.SchemaRefreshCompleted(_logger, tables.Count, _cachedPrompt.Length);
		}
		catch (Exception ex)
		{
			SchemaPromptLogs.SchemaRefreshFailed(_logger, ex);
		}
		finally
		{
			_cacheSemaphore.Release();
		}
	}

	private async Task<Dictionary<string, ClickHouseTable>> FetchTables(
		ClickHouseConnection clickHouseConnection,
		CancellationToken cancellationToken)
	{
		var tables = new Dictionary<string, ClickHouseTable>();

		await using var command = clickHouseConnection.CreateCommand();

		var whereExpression = "is_temporary = 0 AND database IN ({includedDatabases:Array(String)})";
		command.AddParameter("includedDatabases", _includedDatabases);

		if (_excludedEngines.Length != 0)
		{
			whereExpression += " AND engine NOT IN ({excludedEngines:Array(String)})";
			command.AddParameter("excludedEngines", _excludedEngines);
		}

		if (_excludedTables.Length != 0)
		{
			whereExpression += " AND (database, name) NOT IN ({excludedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"excludedTables",
				_excludedTables
					.Select(t => Tuple.Create(t.database, t.table))
					.ToArray());
		}

		if (_includedTables.Length != 0)
		{
			whereExpression += " AND (database, name) IN ({includedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"includedTables",
				_includedTables
					.Select(t => Tuple.Create(t.database, t.table))
					.ToArray());
		}

		command.CommandText = string.Format(SelectTablesSql, whereExpression);

		await using var reader = await command.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			var table = ClickHouseTable.FromReader(reader);
			tables.Add($"{table.Database}.{table.Name}", table);
		}

		return tables;
	}

	private async Task FetchColumns(
		ClickHouseConnection clickHouseConnection,
		Dictionary<string, ClickHouseTable> tables,
		CancellationToken cancellationToken)
	{
		await using var command = clickHouseConnection.CreateCommand();

		var whereExpression = "database IN ({includedDatabases:Array(String)})";
		command.AddParameter("includedDatabases", _includedDatabases);

		if (_excludedTables.Length != 0)
		{
			whereExpression += " AND (database, table) NOT IN ({excludedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"excludedTables",
				_excludedTables
					.Select(t => Tuple.Create(t.database, t.table))
					.ToArray());
		}

		if (_includedTables.Length != 0)
		{
			whereExpression += " AND (database, table) IN ({includedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"includedTables",
				_includedTables
					.Select(t => Tuple.Create(t.database, t.table))
					.ToArray());
		}

		command.CommandText = string.Format(SelectColumnsSql, whereExpression);

		await using var reader = await command.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			var column = ClickHouseColumn.FromReader(reader);

			if (tables.TryGetValue($"{column.Database}.{column.Table}", out var table))
			{
				table.Columns.Add(column);
			}
		}
	}

	private const string SelectTablesSql = """
	                                       SELECT
	                                           database,
	                                           name,
	                                           engine,
	                                           engine_full,
	                                           sorting_key,
	                                           total_rows,
	                                           comment
	                                       FROM system.tables
	                                       WHERE {0}
	                                       ORDER BY database, name
	                                       """;

	private const string SelectColumnsSql = """
	                                        SELECT
	                                            database,
	                                            table,
	                                            name,
	                                            type,
	                                            comment
	                                        FROM system.columns
	                                        WHERE {0}
	                                        ORDER BY database, table, position
	                                        """;

	private const string ClickHouseRules = """
		ClickHouse query rules:
		- Always use fully qualified table names in the form database.table.
		- Use toStartOfInterval or other toStartOf* functions for time bucketing.
		- Only columns defined as Nullable(T) may contain NULL values.
		- Use FINAL only when querying tables with engines: ReplacingMergeTree / SummingMergeTree / AggregatingMergeTree / CollapsingMergeTree / VersionedCollapsingMergeTree / CoalescingMergeTree.
		- Do not use the cluster() / clusterAllReplicas() function.
		- Treat Distributed tables as normal tables; the cluster routing is handled automatically by ClickHouse.
		- When aggregating data from Distributed tables, use normal aggregation functions; ClickHouse will merge partial aggregates from shards automatically.
		""";
}
