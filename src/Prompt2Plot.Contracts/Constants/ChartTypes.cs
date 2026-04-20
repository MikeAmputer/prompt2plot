namespace Prompt2Plot.Contracts.Constants;

public static class ChartTypes
{
	public const string Table = "table";
	public const string Bar = "bar";
	public const string Bubble = "bubble";
	public const string Pie = "pie";

	public static readonly IChartType[] All =
	[
		new BarChartType(),
		new BubbleChartType(),
		new PieChartType(),
		new TableChartType(),
	];
}
