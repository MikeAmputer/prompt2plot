using System.Data;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.ADO.Adapters;
using ClickHouse.Driver.Numerics;
using ClickHouse.Driver.Utility;
using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse;

public sealed class ClickHouseQueryExecutor : DataTableExecutor
{
	private readonly string _connectionString;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly string _httpClientName;

	protected override Dictionary<Type, PlotFieldType> AdditionalTypeMappings => new()
	{
		{ typeof(ClickHouseDecimal), PlotFieldType.Number }
	};

	public ClickHouseQueryExecutor(
		ClickHouseQueryExecutorSettings settings,
		ILoggerFactory? loggerFactory = null)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionSettings.ConnectionString);
		ArgumentNullException.ThrowIfNull(settings.ConnectionSettings.HttpClientFactory);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionSettings.ConnectionString);

		_connectionString = settings.ConnectionSettings.ConnectionString;
		_httpClientFactory = settings.ConnectionSettings.HttpClientFactory;
		_httpClientName = settings.ConnectionSettings.HttpClientName;

		MaxParallelQueries = settings.MaxParallelQueries > 0 ? settings.MaxParallelQueries : ushort.MaxValue;
	}

	protected override async Task<DataTable> ExecuteDataTableAsync(string sqlQuery, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var queryId = Guid.NewGuid().ToString();

		await using var clickHouseConnection = new ClickHouseConnection(
			_connectionString,
			_httpClientFactory,
			_httpClientName);

		await using var command = clickHouseConnection.CreateCommand();
		using var adapter = new ClickHouseDataAdapter();

		command.CommandText = sqlQuery;
		command.QueryId = queryId;
		adapter.SelectCommand = command;

		// TODO: if cancellationToken.CanBeCanceled
		await using var _ = cancellationToken.Register(() => TryKillQuery(queryId));

		var dataTable = new DataTable();
		adapter.Fill(dataTable);

		return dataTable;
	}

	private void TryKillQuery(string queryId)
	{
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
		catch
		{
			// ignore
		}
	}
}
