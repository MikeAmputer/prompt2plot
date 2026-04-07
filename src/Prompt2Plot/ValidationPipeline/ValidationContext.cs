namespace Prompt2Plot;

public sealed class ValidationContext
{
	public required string NaturalLanguageRequest { get; init; }
	public required string Prompt { get; init; }

	public required ModelResponse ModelResponse { get; init; }

	internal bool ShouldRetry { get; private set; }
	public List<string> Errors { get; } = [];
	public List<string> RetryAuxiliaryPrompts { get; } = [];

	public void MarkForRetry()
	{
		ShouldRetry = true;
	}
}
