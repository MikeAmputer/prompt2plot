namespace Prompt2Plot;

public interface IPromptExecutor
{
	Task<ModelResponse?> ExecuteAsync(PromptExecutionContext promptContext, CancellationToken cancellationToken);
}
