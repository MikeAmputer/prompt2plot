using ApexCharts;

namespace Prompt2Plot.Blazor.ApexCharts.Components;

public sealed class ApexBubbleChartSettings
{
	public ApexChartOptions<object[]>? ApexChartOptions { get; set; } = null;

	public bool OverrideTooltip { get; set; } = true;

	public int Height { get; set; } = 350;
	public decimal DefaultRadius { get; set; } = 10;
	public double Opacity { get; set; } = 0.8;
}
