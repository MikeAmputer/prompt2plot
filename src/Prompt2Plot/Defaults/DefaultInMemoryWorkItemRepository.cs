using System.Collections.Concurrent;

namespace Prompt2Plot.Defaults;

/// <summary>
/// A default in-memory implementation of <see cref="IExtendedWorkItemRepository"/>.
/// </summary>
/// <remarks>
/// <para>
/// This repository stores pending work items and completed results in memory using
/// thread-safe collections. It supports optional publishing of work items through
/// <see cref="IWorkItemPublisher"/>. When publishing is enabled and successful,
/// the work item is not stored locally.
/// </para>
/// <para>
/// Both pending work items and results are bounded by configurable limits.
/// When the limits are exceeded, the oldest entries are evicted.
/// </para>
/// <para>
/// The class may be extended. Derived implementations may override
/// configuration properties such as <see cref="UsePublisher"/>, <see cref="MaxPending"/>,
/// and <see cref="MaxResults"/> to customize behavior.
/// </para>
/// </remarks>
public class DefaultInMemoryWorkItemRepository : IExtendedWorkItemRepository
{
	/// <summary>
	/// Gets a value indicating whether the repository should attempt to publish
	/// newly added work items through <see cref="IWorkItemPublisher"/>.
	/// </summary>
	/// <remarks>
	/// If enabled and the publisher successfully accepts the work item,
	/// the item will not be stored in the local pending queue.
	/// </remarks>
	protected virtual bool UsePublisher => true;

	/// <summary>
	/// Gets the maximum number of pending work items stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of pending items exceeds this limit, the oldest items
	/// are removed to keep memory usage bounded.
	/// </remarks>
	protected virtual int MaxPending => 200;

	/// <summary>
	/// Gets the maximum number of completed work item results stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of stored results exceeds this limit, the oldest results
	/// are removed to keep memory usage bounded.
	/// </remarks>
	protected virtual int MaxResults => 200;

	private readonly ConcurrentDictionary<ulong, WorkItem> _pending = new();
	private readonly ConcurrentQueue<ulong> _pendingOrder = new();

	private readonly ConcurrentDictionary<ulong, WorkItemResult> _results = new();
	private readonly ConcurrentQueue<ulong> _resultsOrder = new();

	private long _idCounter;

	private readonly IWorkItemPublisher? _publisher;

	protected DefaultInMemoryWorkItemRepository(IWorkItemPublisher publisher)
	{
		ArgumentNullException.ThrowIfNull(publisher);

		_publisher = publisher;
	}

	/// <inheritdoc />
	public Task<ulong> AddWorkItemAsync(WorkItem workItem, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(workItem);

		var id = (ulong) Interlocked.Increment(ref _idCounter);
		workItem.Id = id;

		if (UsePublisher && _publisher != null)
		{
			if (_publisher.TryPublish(workItem))
			{
				return Task.FromResult(id);
			}
		}

		AddPendingWithEviction(workItem);

		return Task.FromResult(id);
	}

	/// <inheritdoc />
	public Task<IOrderedEnumerable<WorkItem>> GetPendingWorkItemsAsync(CancellationToken cancellationToken)
	{
		var result = _pending
			.Where(x => !_results.ContainsKey(x.Key))
			.Select(x => x.Value)
			.OrderBy(x => x.Id);

		return Task.FromResult(result);
	}

	/// <inheritdoc />
	public Task AddWorkItemResultAsync(WorkItemResult workItemResult, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(workItemResult);

		if (_results.TryAdd(workItemResult.WorkItemId, workItemResult))
		{
			_resultsOrder.Enqueue(workItemResult.WorkItemId);

			EvictExcessiveResults();
		}

		_pending.TryRemove(workItemResult.WorkItemId, out _);

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	/// <exception cref="KeyNotFoundException">
	/// Thrown when a result for the specified <paramref name="workItemId"/> does not exist.
	/// </exception>
	public Task<WorkItemResult> GetWorkItemResultAsync(uint workItemId, CancellationToken cancellationToken)
	{
		if (_results.TryGetValue(workItemId, out var result))
		{
			return Task.FromResult(result);
		}

		throw new KeyNotFoundException($"Result for work item {workItemId} not found.");
	}

	private void AddPendingWithEviction(WorkItem workItem)
	{
		if (_pending.TryAdd(workItem.Id, workItem))
		{
			_pendingOrder.Enqueue(workItem.Id);

			while (_pending.Count > MaxPending && _pendingOrder.TryDequeue(out var oldest))
			{
				_pending.TryRemove(oldest, out _);
			}
		}
	}

	private void EvictExcessiveResults()
	{
		while (_results.Count > MaxResults && _resultsOrder.TryDequeue(out var oldest))
		{
			_results.TryRemove(oldest, out _);
		}
	}
}
