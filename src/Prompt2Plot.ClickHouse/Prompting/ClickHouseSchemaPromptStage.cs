using System.Diagnostics;
using System.Text;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.ClickHouse.Logging;

namespace Prompt2Plot.ClickHouse;

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
			context.Errors.Add("Unable to fetch ClickHouse database schema.");;

			return;
		}

		context.Prompt += _cachedPrompt;
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

			await using var clickHouseConnection = new ClickHouseConnection(
				_connectionString,
				_httpClientFactory,
				_httpClientName);

			var tables = await FetchTables(clickHouseConnection, cancellationToken);
			await FetchColumns(clickHouseConnection, tables, cancellationToken);

			var sb = new StringBuilder();

			sb.AppendLine();
			sb.AppendLine("Database schema:");

			foreach (var tbl in tables.Values)
			{
				sb.Append(tbl.ToPromptString());
				sb.AppendLine();
			}

			sb.AppendLine("Use full table names with database specified.");
			sb.AppendLine();

			_cachedPrompt = sb.ToString();
			_cacheTimer.Restart();
		}
		catch (Exception ex)
		{
			Log.SchemaRefreshFailed(_logger, ex);
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
}
