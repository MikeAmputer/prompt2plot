using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prompt2Plot.Defaults;

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

	public static IServiceCollection AddInMemoryWorkItemRepository(
		this IServiceCollection serviceCollection,
		Func<IServiceProvider, InMemoryWorkItemRepositorySettings> settingsProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentNullException.ThrowIfNull(settingsProvider);

		serviceCollection.AddSingleton<InMemoryWorkItemRepository>(
			sp => new InMemoryWorkItemRepository(
				settingsProvider(sp),
				sp.GetRequiredService<IWorkItemPublisher>(),
				sp.GetService<ILoggerFactory>()));

		return serviceCollection;
	}

	public static IServiceCollection AddInMemoryWorkItemRepository(this IServiceCollection serviceCollection)
	{
		return serviceCollection.AddInMemoryWorkItemRepository(_ => new InMemoryWorkItemRepositorySettings());
	}
}
