namespace Prompt2Plot.Contracts.Visualization;

[Serializable]
public class PlotData
{
	public required string WorkflowKey { get; init; }

	public required string ChartType { get; init; }

	public required string? ChartDescription { get; init; }

	public required PlotDataset[] Datasets { get; init; }
}

[Serializable]
public class PlotDataset
{
	public required string? Label { get; init; }

	public required PlotField[] Fields { get; init; }

	public required List<object?[]> Rows { get; init; }
}

[Serializable]
public class PlotField
{
	public required string Name { get; init; }

	public required string Type { get; init; }
}
