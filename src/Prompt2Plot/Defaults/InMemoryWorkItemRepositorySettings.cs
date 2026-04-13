namespace Prompt2Plot.Defaults;

public sealed class InMemoryWorkItemRepositorySettings
{
	/// <summary>
	/// Gets a value indicating whether the repository should attempt to publish
	/// newly added work items through <see cref="IWorkItemPublisher"/>.
	/// </summary>
	/// <remarks>
	/// If enabled and the publisher successfully accepts the work item,
	/// the item will not be stored in the local pending queue.
	/// </remarks>
	public bool UsePublisher { get; init; } = true;

	/// <summary>
	/// Gets the maximum number of pending work items stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of pending items exceeds this limit, the oldest items
	/// are removed to keep memory usage bounded.
	/// Set to <c>0</c> to disable the limit.
	/// </remarks>
	public uint MaxPending { get; init; } = 200;

	/// <summary>
	/// Gets the maximum number of completed work item results stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of stored results exceeds this limit, the oldest results
	/// are removed to keep memory usage bounded.
	/// Set to <c>0</c> to disable the limit.
	/// </remarks>
	public uint MaxResults { get; init; } = 200;

	/// <summary>
	/// Gets the maximum number of result waiters stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of waiters exceeds this limit, the oldest waiters
	/// are removed and completed with an exception.
	/// Set to <c>0</c> to disable the limit.
	/// </remarks>
	public uint MaxWaiters { get; init; } = 200;
}
