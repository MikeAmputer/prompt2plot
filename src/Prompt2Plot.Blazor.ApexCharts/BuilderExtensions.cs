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


	public static PlotComponentRegistryBuilder WithApexBubbleChartComponent(
		this PlotComponentRegistryBuilder builder,
		ApexBubbleChartSettings settings)
	{
		return builder.WithComponent<ApexBubbleChartComponent, ApexBubbleChartSettings>(ChartTypes.Bubble, settings);
	}

	public static PlotComponentRegistryBuilder WithApexBubbleChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey,
		ApexBubbleChartSettings settings)
	{
		return builder.WithComponent<ApexBubbleChartComponent, ApexBubbleChartSettings>(
			ChartTypes.Bubble, flowKey, settings);
	}

	public static PlotComponentRegistryBuilder WithApexBubbleChartComponent(
		this PlotComponentRegistryBuilder builder)
	{
		return builder.WithApexBubbleChartComponent(new ApexBubbleChartSettings());
	}

	public static PlotComponentRegistryBuilder WithApexBubbleChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey)
	{
		return builder.WithApexBubbleChartComponent(flowKey, new ApexBubbleChartSettings());
	}


	public static PlotComponentRegistryBuilder WithApexLineChartComponent(
		this PlotComponentRegistryBuilder builder,
		ApexLineChartSettings settings)
	{
		return builder.WithComponent<ApexLineChartComponent, ApexLineChartSettings>(ChartTypes.Line, settings);
	}

	public static PlotComponentRegistryBuilder WithApexLineChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey,
		ApexLineChartSettings settings)
	{
		return builder.WithComponent<ApexLineChartComponent, ApexLineChartSettings>(
			ChartTypes.Line, flowKey, settings);
	}

	public static PlotComponentRegistryBuilder WithApexLineChartComponent(
		this PlotComponentRegistryBuilder builder)
	{
		return builder.WithApexLineChartComponent(new ApexLineChartSettings());
	}

	public static PlotComponentRegistryBuilder WithApexLineChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey)
	{
		return builder.WithApexLineChartComponent(flowKey, new ApexLineChartSettings());
	}


	public static PlotComponentRegistryBuilder WithApexPieChartComponent(
		this PlotComponentRegistryBuilder builder,
		ApexPieChartSettings settings)
	{
		return builder.WithComponent<ApexPieChartComponent, ApexPieChartSettings>(ChartTypes.Pie, settings);
	}

	public static PlotComponentRegistryBuilder WithApexPieChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey,
		ApexPieChartSettings settings)
	{
		return builder.WithComponent<ApexPieChartComponent, ApexPieChartSettings>(
			ChartTypes.Pie, flowKey, settings);
	}

	public static PlotComponentRegistryBuilder WithApexPieChartComponent(
		this PlotComponentRegistryBuilder builder)
	{
		return builder.WithApexPieChartComponent(new ApexPieChartSettings());
	}

	public static PlotComponentRegistryBuilder WithApexPieChartComponent(
		this PlotComponentRegistryBuilder builder,
		string flowKey)
	{
		return builder.WithApexPieChartComponent(flowKey, new ApexPieChartSettings());
	}
}
