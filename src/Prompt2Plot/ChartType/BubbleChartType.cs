namespace Prompt2Plot;

/// <summary>
/// Represents a bubble chart visualization.
/// </summary>
/// <remarks>
/// Bubble charts visualize relationships between three numeric variables.
/// Each point is represented by a circle whose position and size encode data.
///
/// Expected dataset fields:
/// <list type="bullet">
/// <item><description><c>x</c> — horizontal coordinate</description></item>
/// <item><description><c>y</c> — vertical coordinate</description></item>
/// <item><description><c>r</c> — bubble radius (size)</description></item>
/// </list>
/// </remarks>
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
