using System.Diagnostics;

namespace Prompt2Plot;

[DebuggerDisplay("{ToPromptString()}")]
public class TableChartType : IChartType
{
	public string Name => "table";

	public string ToPromptString()
	{
		return "'table' with any number of fields, unable to have more than 1 dataset";
	}
}
