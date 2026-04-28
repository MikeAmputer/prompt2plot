using System.Diagnostics;
using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts;

/// <summary>
/// Represents a fallback visualization when the user request cannot be
/// answered using the provided database schema.
///
/// This chart type indicates that no reliable SQL query can be produced.
/// In this case the model must not generate any datasets.
///
/// Typical reasons:
/// <list type="bullet">
/// <item><description>The request references fields not present in the schema</description></item>
/// <item><description>The request requires relationships not defined in the schema</description></item>
/// <item><description>The request cannot produce a meaningful dataset</description></item>
/// </list>
///
/// When this chart type is returned:
/// <list type="bullet">
/// <item><description>No SQL queries must be generated</description></item>
/// <item><description>The <c>Datasets</c> array must be empty</description></item>
/// </list>
/// </summary>
[DebuggerDisplay("{ToPromptString()}")]
public class NoneChartType : IChartType
{
	public string Name => ChartTypes.None;

	public string ToPromptString()
	{
		return
			$"'{ChartTypes.None}' when the request cannot be answered using the provided schema; return zero datasets and do not generate SQL";
	}
}
