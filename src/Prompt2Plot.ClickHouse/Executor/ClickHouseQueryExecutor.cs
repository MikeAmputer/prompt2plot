using System.Data;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.ADO.Adapters;
using ClickHouse.Driver.Numerics;
using ClickHouse.Driver.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.ClickHouse.Logging;
using Prompt2Plot.Contracts;

namespace Prompt2Plot.ClickHouse;

/// <summary>
/// Executes SQL queries against ClickHouse and converts the results
/// into <see cref="DatabaseResponse"/> objects suitable for visualization.
/// </summary>
/// <remarks>
/// This executor is used by Prompt2Plot workflows to retrieve datasets
/// produced by model-generated SQL queries.
///
/// Queries are executed using the ClickHouse HTTP driver and results are
/// loaded into a <see cref="DataTable"/> before being transformed into
/// visualization-friendly structures.
///
/// Features:
///
/// <list type="bullet">
/// <item><description>Automatic type mapping from ClickHouse types to <see cref="PlotFieldType"/></description></item>
/// <item><description>Support for concurrent dataset execution</description></item>
/// <item><description>Query cancellation via ClickHouse <c>KILL QUERY</c></description></item>
/// <item><description>Safe execution with query identifiers for observability</description></item>
/// </list>
/// </remarks>
public sealed class ClickHouseQueryExecutor : DataTableExecutor
{
	private readonly string _connectionString;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly string _httpClientName;

	private readonly ILogger _logger;

	protected override Dictionary<Type, PlotFieldType> AdditionalTypeMappings => new()
	{
		{ typeof(ClickHouseDecimal), PlotFieldType.Number }
	};

	public ClickHouseQueryExecutor(
		ClickHouseQueryExecutorSettings settings,
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

		MaxParallelQueries = settings.MaxParallelQueries > 0 ? settings.MaxParallelQueries : ushort.MaxValue;

		var logFactory = loggerFactory ?? NullLoggerFactory.Instance;
		_logger = logFactory.CreateLogger<ClickHouseQueryExecutor>();
	}

	/// <inheritdoc />
	/// <remarks>
	/// A unique query identifier is generated for each execution. If the
	/// <paramref name="cancellationToken"/> is triggered, the executor attempts
	/// to terminate the running query using a ClickHouse <c>KILL QUERY</c> command.
	/// </remarks>
	protected override async Task<DataTable> ExecuteDataTableAsync(string sqlQuery, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var queryId = Guid.NewGuid().ToString();

		QueryExecutionLogs.QueryExecutionStarted(_logger, queryId);

		try
		{
			await using var clickHouseConnection = new ClickHouseConnection(
				_connectionString,
				_httpClientFactory,
				_httpClientName);

			await using var command = clickHouseConnection.CreateCommand();
			using var adapter = new ClickHouseDataAdapter();

			command.CommandText = sqlQuery;
			command.QueryId = queryId;
			adapter.SelectCommand = command;

			await using var _ = cancellationToken.CanBeCanceled
				? cancellationToken.Register(() => TryKillQuery(queryId))
				: default;

			var dataTable = new DataTable();
			adapter.Fill(dataTable);

			QueryExecutionLogs.QueryExecutionCompleted(_logger, queryId, dataTable.Rows.Count, dataTable.Columns.Count);

			return dataTable;
		}
		catch (Exception ex)
		{
			QueryExecutionLogs.QueryExecutionFailed(_logger, queryId, ex);

			// will be caught and saved as error in DataTableExecutor
			throw;
		}
	}

	private void TryKillQuery(string queryId)
	{
		QueryExecutionLogs.QueryCancellationRequested(_logger, queryId);

		try
		{
			using var clickHouseConnection = new ClickHouseConnection(
				_connectionString,
				_httpClientFactory,
				_httpClientName);

			using var command = clickHouseConnection.CreateCommand();

			command.CommandText = "kill query where query_id = {queryId:String}";
			command.AddParameter("queryId", queryId);

			command.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			QueryExecutionLogs.QueryCancellationFailed(_logger, queryId, ex);
		}
	}
}
