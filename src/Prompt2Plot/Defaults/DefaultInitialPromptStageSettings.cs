using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Defaults;

/// <summary>
/// Provides configuration for <see cref="DefaultInitialPromptStage"/>.
/// </summary>
public sealed class DefaultInitialPromptStageSettings
{
	/// <summary>
	/// The SQL dialect used by the target database (for example: <c>ClickHouse</c>,
	/// <c>PostgreSQL</c>, or <c>MySQL</c>).
	/// </summary>
	public required string SqlDialect { get; init; }

	/// <summary>
	/// Chart types that are supported by consumer visualization service.
	/// </summary>
	public IChartType[] SupportedChartTypes { get; init; } = ChartTypes.All;
}
