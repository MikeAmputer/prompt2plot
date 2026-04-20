namespace Prompt2Plot.Contracts.Visualization;

public static class PlotDatasetExtensions
{
	public static int? IndexOf(this PlotDataset dataset, string fieldName)
		=> FindFirstIndex(dataset, f => string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));

	public static int? FirstNumericIndex(this PlotDataset dataset)
		=> FindFirstIndex(dataset, f => f.IsNumeric());

	public static int? FirstTemporalIndex(this PlotDataset dataset)
		=> FindFirstIndex(dataset, f => f.IsTemporal());

	public static int? FirstCategoricalIndex(this PlotDataset dataset)
		=> FindFirstIndex(dataset, f => f.IsCategorical());

	private static int? FindFirstIndex(PlotDataset dataset, Func<PlotField, bool> predicate)
	{
		for (var i = 0; i < dataset.Fields.Length; i++)
		{
			if (predicate(dataset.Fields[i]))
			{
				return i;
			}
		}

		return null;
	}
}
