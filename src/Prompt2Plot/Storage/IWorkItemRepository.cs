namespace Prompt2Plot;

public interface IWorkItemRepository
{
	Task<IOrderedEnumerable<WorkItem>> GetPendingWorkItemsAsync(CancellationToken cancellationToken);
	Task AddWorkItemResultAsync(WorkItemResult workItemResult, CancellationToken cancellationToken);
}
