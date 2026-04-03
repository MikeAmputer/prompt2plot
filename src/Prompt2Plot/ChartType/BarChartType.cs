namespace Prompt2Plot;

public class BarChartType : ChartTypeBase
{
	public override string Name => "bar";

	public override Dictionary<string, string> Fields => new()
	{
		{"label", "string"},
		{"value", "number"},
	};
}
