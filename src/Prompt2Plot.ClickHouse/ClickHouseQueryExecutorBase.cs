using System.Data;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.ADO.Adapters;
using ClickHouse.Driver.Numerics;
using ClickHouse.Driver.Utility;

namespace Prompt2Plot.ClickHouse;

public abstract class ClickHouseQueryExecutorBase : DataTableExecutor
{
	protected abstract string ConnectionString { get; }
	protected abstract IHttpClientFactory HttpClientFactory { get; }
	protected abstract string HttpClientName { get; }

	protected override Dictionary<Type, PlotFieldType> AdditionalTypeMappings => new()
	{
		{ typeof(ClickHouseDecimal), PlotFieldType.Number }
	};

	protected override async Task<DataTable> ExecuteDataTableAsync(string sqlQuery, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var queryId = Guid.NewGuid().ToString();

		await using var clickHouseConnection = new ClickHouseConnection(
			ConnectionString,
			HttpClientFactory,
			HttpClientName);

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
				ConnectionString,
				HttpClientFactory,
				HttpClientName);

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
