using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Prompt2Plot.Defaults;

public class DefaultInMemoryWorkItemRepository : IExtendedWorkItemRepository
{
	private readonly ConcurrentDictionary<ulong, WorkItem> _pending = new();
	private readonly ConcurrentDictionary<ulong, WorkItemResult> _results = new();

	private int _idCounter;

	private readonly IWorkItemPublisher? _publisher;
	protected virtual bool UsePublisher => true;

	protected DefaultInMemoryWorkItemRepository(IWorkItemPublisher publisher)
	{
		ArgumentNullException.ThrowIfNull(publisher);

		_publisher = publisher;
	}

	public virtual Task<ulong> AddWorkItemAsync(WorkItem workItem, CancellationToken cancellationToken)
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

		_pending.TryAdd(id, workItem);

		return Task.FromResult(id);
	}

	public virtual Task<IOrderedEnumerable<WorkItem>> GetPendingWorkItemsAsync(CancellationToken cancellationToken)
	{
		var result = _pending
			.Where(x => !_results.ContainsKey(x.Key))
			.Select(x => x.Value)
			.OrderBy(x => x.Id);

		return Task.FromResult(result);
	}

	public virtual Task AddWorkItemResultAsync(WorkItemResult workItemResult, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(workItemResult);

		_results.TryAdd(workItemResult.WorkItemId, workItemResult);

		_pending.TryRemove(workItemResult.WorkItemId, out _);

		return Task.CompletedTask;
	}

	public virtual Task<WorkItemResult> GetWorkItemResultAsync(uint workItemId, CancellationToken cancellationToken)
	{
		if (_results.TryGetValue(workItemId, out var result))
		{
			return Task.FromResult(result);
		}

		throw new KeyNotFoundException($"Result for work item {workItemId} not found.");
	}
}
