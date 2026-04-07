namespace Prompt2Plot;

public sealed class PromptContext
{
	public required string NaturalLanguageRequest { get; init; }
	public string Prompt { get; set; } = string.Empty;
	public List<string> Errors { get; set; } = [];
}
