namespace Prompt2Plot;

public interface IValidationPipelineStage
{
	Task ExecuteAsync(ValidationContext context, CancellationToken cancellationToken);
}
