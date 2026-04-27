namespace Prompt2Plot;

public sealed class ValidationContext
{
	public required ulong WorkItemId { get; init; }

	public required ModelResponse ModelResponse { get; init; }

	internal bool ShouldRetry { get; private set; }
	public List<string> Errors { get; } = [];
	public List<string> RetryAuxiliaryPrompts { get; } = [];

	public void MarkForRetry()
	{
		ShouldRetry = true;
	}
}
