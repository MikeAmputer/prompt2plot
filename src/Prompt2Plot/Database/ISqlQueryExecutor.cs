namespace Prompt2Plot;

/// <summary>
/// Executes SQL queries and returns results formatted for visualization.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for executing SQL queries
/// produced by Prompt2Plot workflows and returning the results in a structured
/// format that can be consumed by visualization layers.
/// </remarks>
public interface ISqlQueryExecutor
{
	/// <summary>
	/// Gets the maximum number of SQL queries that can run concurrently
	/// when executing datasets for a single work item.
	/// </summary>
	/// <remarks>
	/// Each model response may produce multiple datasets, each associated with a SQL query.
	/// This value limits how many of those queries may run in parallel for a single work item.
	/// </remarks>
	ushort MaxParallelQueries { get; }

	/// <summary>
	/// Executes a SQL query and returns the resulting dataset.
	/// </summary>
	/// <param name="sqlQuery">The SQL query to execute.</param>
	/// <param name="cancellationToken">A token used to cancel the query execution.</param>
	/// <returns>
	/// A <see cref="DatabaseResponse"/> containing the dataset produced by the query
	/// or an error if execution failed.
	/// </returns>
	Task<DatabaseResponse> ExecuteAsync(string sqlQuery, CancellationToken cancellationToken);
}
