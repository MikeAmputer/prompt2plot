namespace Prompt2Plot;

public sealed class PromptExecutionContext
{
	public required ulong WorkItemId { get; init; }
	public required string NaturalLanguageRequest { get; init; }
	public required string Prompt { get; init; }
	public List<string> Errors { get; } = [];
}
