using System.Collections.Concurrent;

namespace Prompt2Plot.Defaults;

/// <summary>
/// A default in-memory implementation of <see cref="IWorkItemRepository"/>.
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
/// Work item results can be consumed in two ways:
/// <list type="bullet">
/// <item>
/// <description>
/// By retrieving an already completed result using <see cref="GetWorkItemResult(ulong)"/>.
/// </description>
/// </item>
/// <item>
/// <description>
/// By asynchronously awaiting completion of a specific work item using
/// <see cref="WaitForResultAsync(ulong, CancellationToken)"/>.
/// </description>
/// </item>
/// </list>
/// </para>
/// <para>
/// The class may be extended. Derived implementations may override
/// configuration properties such as <see cref="UsePublisher"/>, <see cref="MaxPending"/>,
/// <see cref="MaxResults"/>, and <see cref="MaxWaiters"/> to customize behavior.
/// </para>
/// </remarks>
public class DefaultInMemoryWorkItemRepository : IWorkItemRepository
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

	/// <summary>
	/// Gets the maximum number of result waiters stored in memory.
	/// </summary>
	/// <remarks>
	/// When the number of waiters exceeds this limit, the oldest waiters
	/// are removed and completed with an exception.
	/// </remarks>
	protected virtual int MaxWaiters => 200;

	private readonly ConcurrentDictionary<ulong, WorkItem> _pending = new();
	private readonly ConcurrentQueue<ulong> _pendingOrder = new();
#if NET9_0_OR_GREATER
	private readonly Lock _pendingCompactionLock = new();
#else
	private readonly object _pendingCompactionLock = new();
#endif

	private readonly ConcurrentDictionary<ulong, WorkItemResult> _results = new();
	private readonly ConcurrentQueue<ulong> _resultsOrder = new();

	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<WorkItemResult>> _waiters = new();
	private readonly ConcurrentQueue<ulong> _waitersOrder = new();
#if NET9_0_OR_GREATER
	private readonly Lock _waiterCompactionLock = new();
#else
	private readonly object _waiterCompactionLock = new();
#endif

	private long _idCounter;

	private readonly IWorkItemPublisher? _publisher;

	private const int CompactionQueueSizeMultiplier = 10;

	public DefaultInMemoryWorkItemRepository(IWorkItemPublisher publisher)
	{
		ArgumentNullException.ThrowIfNull(publisher);

		_publisher = publisher;
	}

	/// <summary>
	/// Adds a new work item to the repository.
	/// </summary>
	/// <param name="workItem">The work item to add.</param>
	/// <returns>The identifier assigned to the added work item.</returns>
	public ulong AddWorkItem(WorkItem workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem);

		var id = (ulong) Interlocked.Increment(ref _idCounter);
		workItem.Id = id;

		if (UsePublisher && _publisher != null)
		{
			if (_publisher.TryPublish(workItem))
			{
				return id;
			}
		}

		AddPendingWithEviction(workItem);

		return id;
	}

	/// <inheritdoc />
	public Task<IOrderedEnumerable<WorkItem>> GetPendingWorkItemsAsync(CancellationToken cancellationToken)
	{
		var result = _pending
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

		if (_waiters.TryRemove(workItemResult.WorkItemId, out var waiter))
		{
			waiter.TrySetResult(workItemResult);
		}

		_pending.TryRemove(workItemResult.WorkItemId, out _);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Retrieves the result of the specified work item if it has been completed.
	/// </summary>
	/// <param name="workItemId">
	/// The identifier of the work item whose result should be retrieved.
	/// </param>
	/// <returns>
	/// The <see cref="WorkItemResult"/> associated with the specified work item,
	/// or <see langword="null"/> if the work item has not completed or no result is stored.
	/// </returns>
	public WorkItemResult? GetWorkItemResult(ulong workItemId)
	{
		_results.TryGetValue(workItemId, out var result);

		return result;
	}

	/// <summary>
	/// Asynchronously waits until a result becomes available for the specified work item.
	/// </summary>
	/// <param name="workItemId">
	/// The identifier of the work item whose result should be awaited.
	/// </param>
	/// <param name="cancellationToken">
	/// A token that can be used to cancel the wait operation.
	/// </param>
	/// <returns>
	/// A task that completes with the corresponding <see cref="WorkItemResult"/>
	/// once it becomes available.
	/// </returns>
	/// <remarks>
	/// <para>
	/// If the result already exists at the time of the call, the returned task
	/// completes immediately.
	/// </para>
	/// <para>
	/// Otherwise, the method registers a temporary waiter that is completed when
	/// <see cref="AddWorkItemResultAsync"/> is called for the same work item.
	/// </para>
	/// <para>
	/// The number of concurrent waiters is bounded by <see cref="MaxWaiters"/>.
	/// When this limit is exceeded, the oldest waiters are evicted and their
	/// tasks complete with an exception.
	/// </para>
	/// </remarks>
	/// <exception cref="OperationCanceledException">
	/// Thrown if the provided <paramref name="cancellationToken"/> is cancelled
	/// before the result becomes available.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the waiter is evicted because the repository exceeded
	/// <see cref="MaxWaiters"/>.
	/// </exception>
	public Task<WorkItemResult> WaitForResultAsync(ulong workItemId, CancellationToken cancellationToken)
	{
		if (_results.TryGetValue(workItemId, out var existing))
		{
			return Task.FromResult(existing);
		}

		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled<WorkItemResult>(cancellationToken);
		}

		var waiter = _waiters.GetOrAdd(workItemId, id =>
		{
			var tcs = new TaskCompletionSource<WorkItemResult>(TaskCreationOptions.RunContinuationsAsynchronously);

			_waitersOrder.Enqueue(id);

			EvictExcessiveWaiters();

			return tcs;
		});

		if (_results.TryGetValue(workItemId, out existing))
		{
			if (_waiters.TryRemove(workItemId, out var removed))
			{
				removed.TrySetResult(existing);

				return removed.Task;
			}

			return waiter.Task;
		}

		if (cancellationToken.CanBeCanceled)
		{
			var state = new WaiterCancellationState
			{
				Repo = this,
				WorkItemId = workItemId,
				Token = cancellationToken
			};

			var registration = cancellationToken.Register(WaiterCancellationCallback, state);

			_ = waiter.Task.DisposeRegistrationOnCompletion(registration);
		}

		return waiter.Task;
	}

	private sealed class WaiterCancellationState
	{
		public required DefaultInMemoryWorkItemRepository Repo;
		public ulong WorkItemId;
		public CancellationToken Token;
	}

	private static readonly Action<object?> WaiterCancellationCallback = static state =>
	{
		var s = (WaiterCancellationState) state!;

		if (s.Repo._waiters.TryRemove(s.WorkItemId, out var removed))
		{
			removed.TrySetCanceled(s.Token);
		}
	};

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

		CompactPendingQueueIfNeeded();
	}

	private void EvictExcessiveResults()
	{
		while (_results.Count > MaxResults && _resultsOrder.TryDequeue(out var oldest))
		{
			_results.TryRemove(oldest, out _);
		}
	}

	private void EvictExcessiveWaiters()
	{
		while (_waiters.Count > MaxWaiters && _waitersOrder.TryDequeue(out var oldest))
		{
			if (_waiters.TryRemove(oldest, out var waiter))
			{
				waiter.TrySetException(
					new InvalidOperationException("Result waiter evicted due to repository limits."));
			}
		}

		CompactWaiterQueueIfNeeded();
	}

	private void CompactWaiterQueueIfNeeded()
	{
		if (_waitersOrder.Count < MaxWaiters * CompactionQueueSizeMultiplier)
		{
			return;
		}

		lock (_waiterCompactionLock)
		{
			if (_waitersOrder.Count < MaxWaiters * CompactionQueueSizeMultiplier)
			{
				return;
			}

			_waitersOrder.Clear();

			foreach (var kv in _waiters)
			{
				_waitersOrder.Enqueue(kv.Key);
			}
		}
	}

	private void CompactPendingQueueIfNeeded()
	{
		if (_pendingOrder.Count < MaxPending * CompactionQueueSizeMultiplier)
		{
			return;
		}

		lock (_pendingCompactionLock)
		{
			if (_pendingOrder.Count < MaxPending * CompactionQueueSizeMultiplier)
			{
				return;
			}

			_pendingOrder.Clear();

			foreach (var kv in _pending)
			{
				_pendingOrder.Enqueue(kv.Key);
			}
		}
	}
}
