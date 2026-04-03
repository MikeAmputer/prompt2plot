namespace Prompt2Plot;

internal sealed class PromptPipeline
{
	private readonly List<IPromptPipelineStage> _stages;

	public PromptPipeline(List<IPromptPipelineStage> stages)
	{
		ArgumentNullException.ThrowIfNull(stages);

		_stages = stages;
	}

	public async Task RunAsync(PromptContext context, CancellationToken cancellationToken)
	{
		foreach (var stage in _stages)
		{
			await stage.ExecuteAsync(context, cancellationToken);
		}
	}
}
