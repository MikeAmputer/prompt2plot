using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.Logging;

namespace Prompt2Plot;

/// <summary>
/// Base implementation of <see cref="ISqlQueryExecutor"/> that converts
/// database results stored in a <see cref="DataTable"/> into a
/// <see cref="DatabaseResponse"/>.
/// </summary>
/// <remarks>
/// This class simplifies implementation of SQL executors by handling the
/// transformation of tabular database results into the visualization-friendly
/// structure used by Prompt2Plot.
///
/// Derived classes are responsible only for executing the SQL query and
/// returning a populated <see cref="DataTable"/> via
/// <see cref="ExecuteDataTableAsync(string, CancellationToken)"/>.
///
/// The base implementation performs:
///
/// <list type="bullet">
/// <item><description>Column metadata extraction</description></item>
/// <item><description>Type mapping to <see cref="PlotFieldType"/></description></item>
/// <item><description>Row conversion to positional arrays</description></item>
/// <item><description>Error handling and logging</description></item>
/// </list>
/// </remarks>
public abstract class DataTableExecutor : ISqlQueryExecutor
{
	/// <summary>
	/// Executes a SQL query and returns the results as a <see cref="DataTable"/>.
	/// </summary>
	/// <param name="sqlQuery">The SQL query to execute.</param>
	/// <param name="cancellationToken">A token used to cancel the query execution.</param>
	/// <returns>A populated <see cref="DataTable"/> containing query results.</returns>
	protected abstract Task<DataTable> ExecuteDataTableAsync(string sqlQuery, CancellationToken cancellationToken);

	/// <summary>
	/// Gets additional mappings between database CLR types and <see cref="PlotFieldType"/>.
	/// </summary>
	/// <remarks>
	/// Derived executors may override this property to provide mappings for
	/// database-specific types that are not handled by the default mapping logic.
	/// </remarks>
	protected virtual Dictionary<Type, PlotFieldType> AdditionalTypeMappings => [];

	/// <inheritdoc />
	public ushort MaxParallelQueries { get; protected init; } = 1;

	private readonly ILogger _logger;

	protected DataTableExecutor(ILoggerFactory? loggerFactory = null)
	{
		var logFactory = loggerFactory ?? NullLoggerFactory.Instance;
		_logger = logFactory.CreateLogger<DataTableExecutor>();
	}

	/// <inheritdoc />
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
				var values = new object?[dataTable.Columns.Count];

				for (var i = 0; i < dataTable.Columns.Count; i++)
				{
					values[i] = row[i];
				}

				response.Rows.Add(values);
			}

			return response;
		}
		catch (Exception ex)
		{
			SqlQueryExecutor.QueryExecutionFailed(_logger, ex);

			return new DatabaseResponse
			{
				Error = ex.Message,
			};
		}
	}
}
