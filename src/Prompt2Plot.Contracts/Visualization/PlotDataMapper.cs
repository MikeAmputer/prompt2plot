namespace Prompt2Plot.Contracts.Visualization;

internal static class PlotDataMapper
{
	public static PlotData? Map(WorkItemResult result)
	{
		if (!result.Success || string.IsNullOrWhiteSpace(result.ChartType))
		{
			return null;
		}

		var datasets = result.Datasets
			.Select(MapDataset)
			.Where(d => d != null)
			.ToArray();

		if (datasets.Length == 0)
		{
			return null;
		}

		return new PlotData
		{
			WorkflowKey = result.WorkflowKey,
			ChartType = result.ChartType!,
			ChartDescription = result.ChartDescription,
			Datasets = datasets!
		};
	}

	private static PlotDataset? MapDataset(WorkItemResultDataset dataset)
	{
		if (dataset.Error != null || dataset.Fields == null || dataset.Rows == null)
		{
			return null;
		}

		return new PlotDataset
		{
			Label = dataset.Label,
			Fields = dataset.Fields
				.Select(f => new PlotField
				{
					Name = f.Name,
					Type = MapFieldType(f.Type)
				})
				.ToArray(),
			Rows = dataset.Rows
		};
	}

	private static string MapFieldType(PlotFieldType type)
	{
		return type switch
		{
			PlotFieldType.Boolean => "boolean",
			PlotFieldType.Number => "number",
			PlotFieldType.String => "string",
			PlotFieldType.DateTime => "datetime",
			PlotFieldType.Object => "object",
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}
}
