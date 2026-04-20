using ApexCharts;

namespace Prompt2Plot.Blazor.ApexCharts.Components;

public class ApexBarChartSettings
{
	public ApexChartOptions<object[]>? ApexChartOptions = null;

	public int Height { get; set; } = 350;
	public bool Horizontal { get; set; }
	public string ColumnWidth { get; set; } = "50%";
	public int BorderRadius { get; set; } = 4;
}
