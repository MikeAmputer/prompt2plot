using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot.OpenAI;

public static class BuilderExtensions
{
	public static WorkflowServiceBuilder UseGptStructuredPromptExecutor(
		this WorkflowServiceBuilder builder,
		Func<IServiceProvider, object?, GptPromptExecutorSettings> settingsProvider,
		ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
	{
		builder.ServiceCollection.AddGptStructuredPromptExecutor(
			builder.WorkflowKey,
			settingsProvider,
			serviceLifetime);

		return builder.WithPromptExecutor<GptStructuredPromptExecutor>();
	}

	public static WorkflowServiceBuilder UseGptStructuredPromptExecutor(
		this WorkflowServiceBuilder builder,
		Func<IServiceProvider, GptPromptExecutorSettings> settingsProvider,
		ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
	{
		return builder.UseGptStructuredPromptExecutor(
			(sp, _) => settingsProvider(sp),
			serviceLifetime);
	}
}
