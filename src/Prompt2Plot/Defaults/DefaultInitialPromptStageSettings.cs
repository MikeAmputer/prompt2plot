namespace Prompt2Plot.Defaults;

/// <summary>
/// Provides configuration for <see cref="DefaultInitialPromptStage"/>.
/// </summary>
public sealed class DefaultInitialPromptStageSettings
{
	public required string SqlDialect { get; init; }

	public IChartType[] SupportedChartTypes { get; init; } =
	[
		new BarChartType(),
		new BubbleChartType(),
		new PieChartType(),
		new TableChartType(),
	];
}
