using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts;

public class LineChartType : ChartTypeBase
{
	public override string Name => ChartTypes.Line;

	public override string? AdditionalInfo => "all datasets must use the same category field type";

	public override Dictionary<string, string>[] Fields =>
	[
		new()
		{
			{ PlotFields.Time, PlotFieldTypes.DateTime },
			{ PlotFields.Value, PlotFieldTypes.Number },
		},
		new()
		{
			{ PlotFields.Label, PlotFieldTypes.String },
			{ PlotFields.Value, PlotFieldTypes.Number },
		},
		new()
		{
			{ PlotFields.Label, PlotFieldTypes.Number },
			{ PlotFields.Value, PlotFieldTypes.Number },
		}
	];
}
