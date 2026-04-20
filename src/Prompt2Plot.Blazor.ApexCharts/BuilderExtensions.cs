using Prompt2Plot.Blazor.ApexCharts.Components;
using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Blazor.ApexCharts;

public static class BuilderExtensions
{
	public static PlotComponentRegistryBuilder WithApexBarChartComponent(
		this PlotComponentRegistryBuilder builder,
		ApexBarChartSettings settings)
	{
		return builder.WithComponent<ApexBarChartComponent, ApexBarChartSettings>(ChartTypes.Bar, settings);
	}

	public static PlotComponentRegistryBuilder WithApexBarChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey,
		ApexBarChartSettings settings)
	{
		return builder.WithComponent<ApexBarChartComponent, ApexBarChartSettings>(ChartTypes.Bar, flowKey, settings);
	}

	public static PlotComponentRegistryBuilder WithApexBarChartComponent(
		this PlotComponentRegistryBuilder builder)
	{
		return builder.WithApexBarChartComponent(new ApexBarChartSettings());
	}

	public static PlotComponentRegistryBuilder WithApexBarChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey)
	{
		return builder.WithApexBarChartComponent(flowKey, new ApexBarChartSettings());
	}
}
