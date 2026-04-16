namespace Prompt2Plot.InMemory;

/// <summary>
/// Provides configuration for <see cref="InMemoryWorkItemRepository"/>.
/// </summary>
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
	/// </remarks>
	/// <value>
	/// Set to <c>0</c> to disable in-memory storage of pending work items.
	/// </value>
	public uint MaxPending { get; init; } = 64;

	/// <summary>
	/// Gets the maximum number of completed work item results stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of stored results exceeds this limit, the oldest results
	/// are removed to keep memory usage bounded.
	/// </remarks>
	/// <value>
	/// Set to <c>0</c> to disable result storage. In this mode
	/// <see cref="InMemoryWorkItemRepository.GetWorkItemResult(ulong)"/> cannot be used.
	/// </value>
	public uint MaxResults { get; init; } = 64;

	/// <summary>
	/// Gets the maximum number of result waiters stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of waiters exceeds this limit, the oldest waiters
	/// are removed and completed with an exception.
	/// </remarks>
	/// <value>
	/// Set to <c>0</c> to disable waiter registration. In this mode
	/// <see cref="InMemoryWorkItemRepository.WaitForResultAsync(ulong, CancellationToken)"/>
	/// cannot be used.
	/// </value>
	public uint MaxWaiters { get; init; } = 128;

	/// <summary>
	/// Gets the capacity of the result streaming channel.
	/// </summary>
	/// <remarks>
	/// This channel is used by
	/// <see cref="InMemoryWorkItemRepository.ConsumeResultsAsync(CancellationToken)"/>
	/// to stream completed results to consumers.
	/// When the channel reaches capacity, the oldest results are dropped.
	/// </remarks>
	/// <value>
	/// Set to <c>0</c> to disable result streaming.
	/// </value>
	public uint ResultChannelCapacity { get; init; } = 64;
}
