using System.Data;

namespace Prompt2Plot;

public abstract class DataTableExecutor : ISqlQueryExecutor
{
	protected abstract Task<DataTable> ExecuteDataTableAsync(string sqlQuery, CancellationToken cancellationToken);
	protected virtual Dictionary<Type, PlotFieldType> AdditionalTypeMappings => [];

	public async Task<DatabaseResponse> ExecuteAsync(string sqlQuery, CancellationToken cancellationToken)
	{
		try
		{
			var dataTable = await ExecuteDataTableAsync(sqlQuery, cancellationToken);

			var response = new DatabaseResponse();

			foreach (DataColumn column in dataTable.Columns)
			{
				response.Fields.Add(new PlotField
				{
					Name = column.ColumnName,
					Type = column.DataType.MapToPlotFieldType(AdditionalTypeMappings),
				});
			}

			foreach (DataRow row in dataTable.Rows)
			{
				var dict = new Dictionary<string, object?>(dataTable.Columns.Count);
				foreach (DataColumn column in dataTable.Columns)
				{
					dict[column.ColumnName] = row[column];
				}

				response.Data.Add(dict);
			}

			return response;
		}
		catch (Exception ex)
		{
			return new DatabaseResponse
			{
				Error = ex.Message,
			};
		}
	}
}
