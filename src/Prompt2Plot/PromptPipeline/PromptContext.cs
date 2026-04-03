namespace Prompt2Plot;

public sealed class PromptContext
{
	public required string NaturalLanguageRequest { get; init; }
	public string Prompt { get; set; } = string.Empty;
	public string? Error { get; set; }
}
