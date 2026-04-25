using Prompt2Plot.Blazor.Components;
using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Blazor;

public static class BuilderExtensions
{
	public static PlotComponentRegistryBuilder WithQuickGridTableComponent(
		this PlotComponentRegistryBuilder builder,
		QuickGridTableSettings settings)
	{
		return builder.WithComponent<QuickGridTableComponent, QuickGridTableSettings>(ChartTypes.Table, settings);
	}

	public static PlotComponentRegistryBuilder WithQuickGridTableComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey,
		QuickGridTableSettings settings)
	{
		return builder.WithComponent<QuickGridTableComponent, QuickGridTableSettings>(ChartTypes.Table, flowKey, settings);
	}

	public static PlotComponentRegistryBuilder WithQuickGridTableComponent(
		this PlotComponentRegistryBuilder builder)
	{
		return builder.WithQuickGridTableComponent(new QuickGridTableSettings());
	}

	public static PlotComponentRegistryBuilder WithQuickGridTableComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey)
	{
		return builder.WithQuickGridTableComponent(flowKey, new QuickGridTableSettings());
	}
}
