namespace Prompt2Plot;

public interface ISqlQueryExecutor
{
	ushort MaxParallelQueries => 1;

	Task<DatabaseResponse> ExecuteAsync(string sqlQuery, CancellationToken cancellationToken);
}
