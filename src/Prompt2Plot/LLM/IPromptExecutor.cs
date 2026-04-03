namespace Prompt2Plot;

public interface IPromptExecutor
{
	Task<ModelResponse> ExecuteAsync(PromptContext promptContext, CancellationToken cancellationToken);
}
