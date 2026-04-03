namespace Prompt2Plot;

public sealed class WorkItem
{
	public ulong Id { get; set; }
	public required string WorkflowKey { get; set; }

	public required string NaturalLanguageRequest { get; set; }
}
