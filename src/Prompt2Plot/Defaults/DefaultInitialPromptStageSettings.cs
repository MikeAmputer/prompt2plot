namespace Prompt2Plot.Defaults;

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
