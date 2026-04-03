namespace Prompt2Plot;


public interface IPromptPipelineStage
{
	Task ExecuteAsync(PromptContext context, CancellationToken cancellationToken);
}
