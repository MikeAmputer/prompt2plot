namespace Prompt2Plot;

internal sealed class ValidationPipeline
{
	private readonly List<IValidationPipelineStage> _stages;
	public required int MaxRetries { get; init; }

	public ValidationPipeline(List<IValidationPipelineStage> stages)
	{
		ArgumentNullException.ThrowIfNull(stages);

		_stages = stages;
	}

	public async Task<ValidationContext> RunAsync(ValidationContext context, CancellationToken cancellationToken)
	{
		foreach (var stage in _stages)
		{
			await stage.ExecuteAsync(context, cancellationToken);
		}
		return context;
	}
}
