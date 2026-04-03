namespace Prompt2Plot;

public sealed class ValidationContext
{
	public required string NaturalLanguageRequest { get; init; }
	public required string Prompt { get; init; }

	public required ModelResponse ModelResponse { get; init; }

	internal bool ShouldRetry { get; private set; }
	public string? Error { get; set; }
	public string RetryAuxiliaryPrompt { get; set; } = string.Empty;

	public void MarkForRetry()
	{
		ShouldRetry = true;
	}
}
