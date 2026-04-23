namespace Prompt2Plot.Contracts.Visualization;

public static class PlotDatasetExtensions
{
	public static (int Index, PlotField Field)? FirstNumeric(this PlotDataset dataset)
		=> FindFirst(dataset, f => f.IsNumeric());

	public static (int Index, PlotField Field)? FirstTemporal(this PlotDataset dataset)
		=> FindFirst(dataset, f => f.IsTemporal());

	public static (int Index, PlotField Field)? FirstCategorical(this PlotDataset dataset)
		=> FindFirst(dataset, f => f.IsCategorical());


	public static IEnumerable<(int Index, PlotField Field)> AllNumeric(this PlotDataset dataset)
		=> FindAll(dataset, f => f.IsNumeric());

	public static IEnumerable<(int Index, PlotField Field)> AllTemporal(this PlotDataset dataset)
		=> FindAll(dataset, f => f.IsTemporal());

	public static IEnumerable<(int Index, PlotField Field)> AllCategorical(this PlotDataset dataset)
		=> FindAll(dataset, f => f.IsCategorical());


	private static (int Index, PlotField Field)? FindFirst(
		PlotDataset dataset,
		Func<PlotField, bool> predicate)
	{
		var fields = dataset.Fields;

		for (var i = 0; i < fields.Length; i++)
		{
			var field = fields[i];
			if (predicate(field))
			{
				return (i, field);
			}
		}

		return null;
	}

	private static IEnumerable<(int Index, PlotField Field)> FindAll(
		PlotDataset dataset,
		Func<PlotField, bool> predicate)
	{
		var fields = dataset.Fields;

		for (var i = 0; i < fields.Length; i++)
		{
			var field = fields[i];
			if (predicate(field))
			{
				yield return (i, field);
			}
		}
	}
}
