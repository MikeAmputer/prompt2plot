namespace Prompt2Plot;

/// <summary>
/// Represents a chart type that can be produced by the Prompt2Plot pipeline.
/// </summary>
/// <remarks>
/// Chart types describe the structure of datasets expected by a visualization.
/// Each implementation defines:
/// <list type="bullet">
/// <item><description>The chart name used by the model response</description></item>
/// <item><description>The required fields expected in the SQL result</description></item>
/// <item><description>An optional tool constraint (e.g., specific charting library)</description></item>
/// </list>
///
/// Chart types are converted to a textual description using
/// <see cref="ToPromptString"/> and embedded into the prompt so that the
/// language model understands the supported visualization formats.
///
/// Implementations should remain deterministic and avoid runtime state.
/// </remarks>
public interface IChartType
{
	/// <summary>
	/// Gets the unique chart type name returned by the model.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Converts the chart type into a textual description suitable for
	/// inclusion in an LLM prompt.
	/// </summary>
	string ToPromptString();
}
