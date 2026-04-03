namespace Prompt2Plot;

internal sealed class WorkItemPublisher : IWorkItemPublisher
{
	private DeduplicatingWorkItemChannel? _channel;
	private readonly ThreadSafeBool _isRunning = new();

	public bool TryPublish(WorkItem workItem)
	{
		if (!_isRunning.Value)
		{
			return false;
		}

		return _channel != null && _channel.TryWrite(workItem);
	}

	public void Start(DeduplicatingWorkItemChannel? channel)
	{
		if (!_isRunning.TrySet(true))
		{
			throw new InvalidOperationException("Publisher has already started.");
		}

		_channel = channel;
	}

	public void TryStop()
	{
		_isRunning.Value = false;
		_channel = null;
	}
}
