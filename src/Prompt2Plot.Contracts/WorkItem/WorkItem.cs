namespace Prompt2Plot.Contracts;

public sealed class WorkItem
{
	public ulong Id { get; set; }
	public required string WorkflowKey { get; set; }

	public required string NaturalLanguageRequest { get; set; }
}
