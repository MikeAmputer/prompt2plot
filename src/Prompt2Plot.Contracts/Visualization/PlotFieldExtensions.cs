using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts.Visualization;

public static class PlotFieldExtensions
{
	public static bool IsNumeric(this PlotField field)
		=> field.Type == PlotFieldTypes.Number;

	public static bool IsTemporal(this PlotField field)
		=> field.Type == PlotFieldTypes.DateTime;

	public static bool IsCategorical(this PlotField field)
		=> field.Type == PlotFieldTypes.String
		   || field.Type == PlotFieldTypes.Boolean;
}
