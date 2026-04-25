using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts;

/// <summary>
/// Represents a pie chart visualization.
/// </summary>
/// <remarks>
/// Pie charts display proportions of a whole using slices whose
/// sizes correspond to numeric values.
///
/// Expected dataset fields:
/// <list type="bullet">
/// <item><description><c>label</c> — category name</description></item>
/// <item><description><c>value</c> — numeric value representing the category portion</description></item>
/// </list>
/// </remarks>
public class PieChartType : ChartTypeBase
{
	public override string Name => ChartTypes.Pie;

	public override string? AdditionalInfo =>
		"should contain a small number of categories; must contain exactly one dataset";

	public override Dictionary<string, string>[] Fields =>
	[
		new()
		{
			{ PlotFields.Label, PlotFieldTypes.String },
			{ PlotFields.Value, PlotFieldTypes.Number },
		}
	];
}
