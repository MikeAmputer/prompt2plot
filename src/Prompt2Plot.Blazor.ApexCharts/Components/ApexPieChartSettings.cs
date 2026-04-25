using ApexCharts;

namespace Prompt2Plot.Blazor.ApexCharts.Components;

public class ApexPieChartSettings
{
	public ApexChartOptions<object[]>? ApexChartOptions { get; set; } = null;

	public int Height { get; set; } = 350;

	public bool ShowLegend { get; set; } = true;

	public bool ShowDataLabels { get; set; } = true;
}
