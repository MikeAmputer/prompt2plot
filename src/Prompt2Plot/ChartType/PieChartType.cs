namespace Prompt2Plot;

public class PieChartType : ChartTypeBase
{
	public override string Name => "pie";

	public override Dictionary<string, string> Fields => new()
	{
		{"label", "string"},
		{"value", "number"},
	};
}
