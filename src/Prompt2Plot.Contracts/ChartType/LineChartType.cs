using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts;

public class LineChartType : ChartTypeBase
{
	public override string Name => ChartTypes.Line;

	public override string? AdditionalInfo =>
		$"'{PlotFields.Time}' must be of type '{PlotFieldTypes.DateTime}' and represent the chronological axis; " +
		$"alternatively a '{PlotFields.Label}' field may be used instead of '{PlotFields.Time}' " +
		"to represent ordered categories, but all datasets must use the same category field type; " +
		"rows in the dataset must be ordered according to the progression of the time or label field";

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
