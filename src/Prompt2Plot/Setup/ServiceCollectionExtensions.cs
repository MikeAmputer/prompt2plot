using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPrompt2Plot(
		this IServiceCollection serviceCollection,
		Action<Prompt2PlotBuilder> setup)
	{
		serviceCollection.AddSingleton<WorkflowFactory>();
		serviceCollection.AddSingleton<IWorkflowExecutionService, WorkflowExecutionService>();

		serviceCollection.AddSingleton<WorkItemPublisher>();
		serviceCollection.AddSingleton<IWorkItemPublisher>(sp => sp.GetRequiredService<WorkItemPublisher>());

		var builder = new Prompt2PlotBuilder();
		setup(builder);
		builder.Build(serviceCollection);

		return serviceCollection;
	}
}
