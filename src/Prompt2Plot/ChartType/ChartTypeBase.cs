using System.Diagnostics;

namespace Prompt2Plot;

[DebuggerDisplay("{ToPromptString()}")]
public abstract class ChartTypeBase : IChartType
{
	public abstract string Name { get; }
	public virtual string? SpecificTool => null;
	public abstract Dictionary<string, string> Fields { get; }
	public virtual string? AdditionalInfo => null;

	public string ToPromptString()
	{
		var toolPart = SpecificTool != null ? $" (for tool: {SpecificTool})" : string.Empty;
		var fieldsPart = string.Join(", ", Fields.Select(f => $"{f.Key}: {f.Value}"));
		var additionalPart = AdditionalInfo != null ? $", {AdditionalInfo}" : string.Empty;
		return $"'{Name}'{toolPart} with fields [{fieldsPart}]{additionalPart}";
	}
}
