using System.Diagnostics;
using System.Text;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.ClickHouse.Logging;

namespace Prompt2Plot.ClickHouse;

public abstract class ClickHouseSchemaPromptStageBase : IPromptPipelineStage
{
	protected abstract string ConnectionString { get; }
	protected abstract IHttpClientFactory HttpClientFactory { get; }
	protected abstract string HttpClientName { get; }

	protected abstract string[] IncludedDatabases { get; }
	protected virtual (string database, string table)[] IncludedTables => [];
	protected virtual (string database, string table)[] ExcludedTables => [];
	protected virtual string[] ExcludedEngines => ["MaterializedView"];

	private volatile string _cachedPrompt = string.Empty;
	private readonly Stopwatch _cacheTimer = new();
	private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
	protected virtual TimeSpan CacheDuration => TimeSpan.FromMinutes(30);

	private readonly ILogger _logger;

	protected ClickHouseSchemaPromptStageBase(ILoggerFactory? loggerFactory = null)
	{
		var logFactory = loggerFactory ?? NullLoggerFactory.Instance;
		_logger = logFactory.CreateLogger<ClickHouseSchemaPromptStageBase>();
	}

	public async Task ExecuteAsync(PromptContext context, CancellationToken cancellationToken)
	{
		if (!_cacheTimer.IsRunning || _cacheTimer.Elapsed >= CacheDuration)
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
			if (_cacheTimer.IsRunning && _cacheTimer.Elapsed < CacheDuration)
			{
				return;
			}

			await using var clickHouseConnection = new ClickHouseConnection(
				ConnectionString,
				HttpClientFactory,
				HttpClientName);

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
		command.AddParameter("includedDatabases", IncludedDatabases);

		if (ExcludedEngines.Length != 0)
		{
			whereExpression += " AND engine NOT IN ({excludedEngines:Array(String)})";
			command.AddParameter("excludedEngines", ExcludedEngines);
		}

		if (ExcludedTables.Length != 0)
		{
			whereExpression += " AND (database, name) NOT IN ({excludedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"excludedTables",
				ExcludedTables
					.Select(t => Tuple.Create(t.database, t.table))
					.ToArray());
		}

		if (IncludedTables.Length != 0)
		{
			whereExpression += " AND (database, name) IN ({includedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"includedTables",
				IncludedTables
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
		command.AddParameter("includedDatabases", IncludedDatabases);

		if (ExcludedTables.Length != 0)
		{
			whereExpression += " AND (database, table) NOT IN ({excludedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"excludedTables",
				ExcludedTables
					.Select(t => Tuple.Create(t.database, t.table))
					.ToArray());
		}

		if (IncludedTables.Length != 0)
		{
			whereExpression += " AND (database, table) IN ({includedTables:Array(Tuple(String, String))})";
			command.AddParameter(
				"includedTables",
				IncludedTables
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
