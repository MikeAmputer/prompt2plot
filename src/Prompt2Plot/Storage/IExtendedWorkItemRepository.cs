namespace Prompt2Plot;

public interface IExtendedWorkItemRepository : IWorkItemRepository
{
	Task<ulong> AddWorkItemAsync(WorkItem workItem, CancellationToken cancellationToken);
	Task<WorkItemResult> GetWorkItemResultAsync(uint workItemId, CancellationToken cancellationToken);
}
