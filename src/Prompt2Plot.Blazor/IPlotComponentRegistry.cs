namespace Prompt2Plot.Blazor;

public interface IPlotComponentRegistry
{
	PlotComponentRegistration? Resolve(string chartType, string flowKey);
}
