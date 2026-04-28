namespace Prompt2Plot.Contracts.Constants;

public static class ChartTypes
{
	public const string None = "none";
	public const string Bar = "bar";
	public const string Line = "line";
	public const string Pie = "pie";
	public const string Table = "table";
	public const string Bubble = "bubble";

	public static readonly IChartType[] All =
	[
		new NoneChartType(),
		new BarChartType(),
		new LineChartType(),
		new PieChartType(),
		new TableChartType(),
		new BubbleChartType(),
	];
}
