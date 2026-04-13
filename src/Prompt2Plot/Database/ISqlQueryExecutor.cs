namespace Prompt2Plot;

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

	Task<DatabaseResponse> ExecuteAsync(string sqlQuery, CancellationToken cancellationToken);
}
