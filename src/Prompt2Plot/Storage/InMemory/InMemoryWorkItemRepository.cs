using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Prompt2Plot.InMemory;

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
/// </remarks>
public sealed class InMemoryWorkItemRepository : IWorkItemRepository
{
	private readonly bool _usePublisher;
	private readonly uint _maxPending;
	private readonly uint _maxResults;
	private readonly uint _maxWaiters;

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

	public InMemoryWorkItemRepository(
		InMemoryWorkItemRepositorySettings settings,
		IWorkItemPublisher publisher,
		ILoggerFactory? loggerFactory = null)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(publisher);

		_usePublisher = settings.UsePublisher;
		_maxPending = settings.MaxPending > 0 ? settings.MaxPending : uint.MaxValue;
		_maxResults = settings.MaxResults > 0 ? settings.MaxResults : uint.MaxValue;
		_maxWaiters = settings.MaxWaiters > 0 ? settings.MaxWaiters : uint.MaxValue;

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

		if (_usePublisher && _publisher != null)
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
		var result = _pending.Values.OrderBy(x => x.Id);

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
	/// The number of concurrent waiters is bounded by <see cref="_maxWaiters"/>.
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
	/// <see cref="_maxWaiters"/>.
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
		public required InMemoryWorkItemRepository Repo;
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

			while (_pending.Count > _maxPending && _pendingOrder.TryDequeue(out var oldest))
			{
				_pending.TryRemove(oldest, out _);
			}
		}

		CompactPendingQueueIfNeeded();
	}

	private void EvictExcessiveResults()
	{
		while (_results.Count > _maxResults && _resultsOrder.TryDequeue(out var oldest))
		{
			_results.TryRemove(oldest, out _);
		}
	}

	private void EvictExcessiveWaiters()
	{
		while (_waiters.Count > _maxWaiters && _waitersOrder.TryDequeue(out var oldest))
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
		if (_waitersOrder.Count < _maxWaiters * CompactionQueueSizeMultiplier)
		{
			return;
		}

		lock (_waiterCompactionLock)
		{
			if (_waitersOrder.Count < _maxWaiters * CompactionQueueSizeMultiplier)
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
		if (_pendingOrder.Count < _maxPending * CompactionQueueSizeMultiplier)
		{
			return;
		}

		lock (_pendingCompactionLock)
		{
			if (_pendingOrder.Count < _maxPending * CompactionQueueSizeMultiplier)
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
