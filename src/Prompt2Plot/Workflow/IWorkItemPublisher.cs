namespace Prompt2Plot;

public interface IWorkItemPublisher
{
	bool TryPublish(WorkItem workItem);
}
