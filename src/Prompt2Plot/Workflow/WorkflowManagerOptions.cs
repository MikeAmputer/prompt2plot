namespace Prompt2Plot;

public sealed class WorkflowManagerOptions
{
	public required int MaxDegreeOfParallelism { get; init; }
	public required TimeSpan PopulateInterval { get; init; }
}
