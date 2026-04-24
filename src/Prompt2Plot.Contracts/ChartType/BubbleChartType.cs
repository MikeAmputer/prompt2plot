using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts;

/// <summary>
/// Represents a bubble chart visualization.
/// </summary>
/// <remarks>
/// Bubble charts visualize relationships between three numeric variables.
/// Each data point is drawn as a circle where:
/// - X determines horizontal position
/// - Y determines vertical position
/// - Size determines bubble size
///
/// Expected dataset fields:
/// <list type="bullet">
/// <item><description><c>x</c> — horizontal coordinate</description></item>
/// <item><description><c>y</c> — vertical coordinate</description></item>
/// <item><description><c>size</c> — bubble size</description></item>
/// </list>
///
/// An additional categorical string field may be included.
/// If present, its name and value may be used to label the data point (e.g., in tooltips).
/// </remarks>
public class BubbleChartType : ChartTypeBase
{
	public override string Name => ChartTypes.Bubble;

	public override string AdditionalInfo =>
		$"where '{PlotFields.X}' and '{PlotFields.Y}' are point coordinates, and '{PlotFields.Value}' is bubble size; " +
		$"numeric fields must appear in the order '{PlotFields.X}', '{PlotFields.Y}', '{PlotFields.Value}'; " +
		"field aliases will be used as labels in the chart; " +
		"a categorical string field may also be included to label the data point";

	public override Dictionary<string, string>[] Fields =>
	[
		new()
		{
			{ PlotFields.X, PlotFieldTypes.Number },
			{ PlotFields.Y, PlotFieldTypes.Number },
			{ PlotFields.Value, PlotFieldTypes.Number },
		},
		new()
		{
			{ PlotFields.Label, PlotFieldTypes.String },
			{ PlotFields.X, PlotFieldTypes.Number },
			{ PlotFields.Y, PlotFieldTypes.Number },
			{ PlotFields.Value, PlotFieldTypes.Number },
		}
	];
}
