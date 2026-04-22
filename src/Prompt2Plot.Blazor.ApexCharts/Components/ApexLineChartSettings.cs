using ApexCharts;

namespace Prompt2Plot.Blazor.ApexCharts.Components;

public class ApexLineChartSettings
{
	public ApexChartOptions<object[]>? ApexChartOptions { get; set; } = null;

	public int Height { get; set; } = 350;
}
