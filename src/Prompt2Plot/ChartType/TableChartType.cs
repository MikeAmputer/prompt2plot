using System.Diagnostics;

namespace Prompt2Plot;

/// <summary>
/// Represents a tabular visualization.
/// </summary>
/// <remarks>
/// Unlike other chart types, table visualizations do not impose a fixed
/// dataset structure. The SQL query may return any number of columns.
///
/// Tables are typically used when:
/// <list type="bullet">
/// <item><description>The data does not map well to a chart</description></item>
/// <item><description>The user requests raw or detailed records</description></item>
/// <item><description>Multiple heterogeneous fields must be displayed</description></item>
/// </list>
///
/// Tables currently support a single dataset.
/// </remarks>
[DebuggerDisplay("{ToPromptString()}")]
public class TableChartType : IChartType
{
	public string Name => "table";

	public string ToPromptString()
	{
		return "'table' with any number of fields, unable to have more than 1 dataset";
	}
}
