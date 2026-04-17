namespace Prompt2Plot;

public sealed class WorkItemResult
{
	public required ulong WorkItemId { get; init; }
	public required bool Success { get; init; }
	public string? ChartType { get; init; }
	public string? ChartDescription { get; init; }

	public List<WorkItemResultDataset> Datasets { get; init; } = [];
	public List<string> Errors { get; init; } = [];
}

public sealed class WorkItemResultDataset
{
	public string? SqlQuery { get; init; }
	public string? Label { get; init; }
	public List<PlotField>? Fields { get; init; }
	public List<object?[]>? Rows { get; init; }
	public string? Error { get; init; }
}
