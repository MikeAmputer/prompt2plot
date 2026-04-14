namespace Prompt2Plot;

/// <summary>
/// Represents a bar chart visualization.
/// </summary>
/// <remarks>
/// Bar charts display categorical values where each category
/// is represented by a bar whose height corresponds to a numeric value.
///
/// Expected dataset fields:
/// <list type="bullet">
/// <item><description><c>label</c> — category name</description></item>
/// <item><description><c>value</c> — numeric value for the category</description></item>
/// </list>
/// </remarks>
public class BarChartType : ChartTypeBase
{
	public override string Name => "bar";

	public override Dictionary<string, string> Fields => new()
	{
		{"label", "string"},
		{"value", "number"},
	};
}
