namespace Prompt2Plot;

public class BubbleChartType : ChartTypeBase
{
	public override string Name => "bubble";
	public override string AdditionalInfo => "where 'x' and 'y' are point coordinates, and 'r' is its radius (size)";

	public override Dictionary<string, string> Fields => new()
	{
		{"x", "number"},
		{"y", "number"},
		{"r", "number"},
	};
}
