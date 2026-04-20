using Prompt2Plot.Contracts.Visualization;

namespace Prompt2Plot.Contracts;

public static class WorkItemExtensions
{
	public static PlotData? AsPlotData(this WorkItemResult workItemResult)
	{
		return PlotDataMapper.Map(workItemResult);
	}
}
