namespace Prompt2Plot.Blazor;

public interface IPlotComponentRegistry
{
	PlotComponentDescriptor? Resolve(string chartType, string flowKey);
}
