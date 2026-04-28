using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot.OpenAI;

public static class BuilderExtensions
{
	/// <summary>
	/// Configures the workflow to use <see cref="GptStructuredPromptExecutor"/>
	/// for prompt execution.
	/// </summary>
	/// <param name="builder">Workflow builder.</param>
	/// <param name="settingsProvider">Factory that produces executor settings.</param>
	/// <param name="serviceLifetime">Service lifetime of the executor.</param>
	/// <returns>The updated workflow builder.</returns>
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

	/// <summary>
	/// Configures the workflow to use <see cref="GptStructuredPromptExecutor"/>
	/// with a simplified settings factory.
	/// </summary>
	/// <param name="builder">Workflow builder.</param>
	/// <param name="settingsProvider">Factory that produces executor settings.</param>
	/// <param name="serviceLifetime">Service lifetime of the executor.</param>
	/// <returns>The updated workflow builder.</returns>
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
