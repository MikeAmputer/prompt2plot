namespace Prompt2Plot.ClickHouse;

public sealed class ClickHouseQueryExecutorSettings
{
	/// <summary>
	/// Gets the connection settings used to create ClickHouse connections.
	/// </summary>
	public required ClickHouseConnectionSettings ConnectionSettings { get; init; }

	/// <summary>
	/// Gets the maximum number of SQL queries that can run concurrently
	/// when executing datasets for a single work item.
	/// </summary>
	/// <remarks>
	/// Each model response may produce multiple datasets, each associated with a SQL query.
	/// This setting limits how many of those queries may run in parallel for a single work item.
	/// Set to <c>0</c> to remove the limit and allow all dataset queries to execute concurrently.
	/// </remarks>
	public ushort MaxParallelQueries { get; init; } = 1;
}
