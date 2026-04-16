using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prompt2Plot.InMemory;

namespace Prompt2Plot;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the core Prompt2Plot services and configures the library using the provided builder.
	/// </summary>
	/// <remarks>
	/// This method registers the infrastructure services required to execute workflows,
	/// including workflow creation, execution, and work item publishing.
	///
	/// The <paramref name="setup"/> delegate is used to configure Prompt2Plot by
	/// registering workflows, pipelines, and repository implementations through
	/// the <see cref="Prompt2PlotBuilder"/>.
	///
	/// This method must be called once during application startup.
	/// </remarks>
	/// <param name="serviceCollection">
	/// The service collection used to register Prompt2Plot services.
	/// </param>
	/// <param name="setup">
	/// A delegate used to configure Prompt2Plot using the <see cref="Prompt2PlotBuilder"/>.
	/// </param>
	/// <returns>
	/// The same <see cref="IServiceCollection"/> instance so that additional services can be registered.
	/// </returns>
	public static IServiceCollection AddPrompt2Plot(
		this IServiceCollection serviceCollection,
		Action<Prompt2PlotBuilder> setup)
	{
		serviceCollection.ThrowIfRegistered<WorkflowFactory>();
		serviceCollection.ThrowIfRegistered<IWorkflowExecutionService>();
		serviceCollection.ThrowIfRegistered<WorkItemPublisher>();
		serviceCollection.ThrowIfRegistered<IWorkItemPublisher>();

		serviceCollection.AddSingleton<WorkflowFactory>();
		serviceCollection.AddSingleton<IWorkflowExecutionService, WorkflowExecutionService>();

		serviceCollection.AddSingleton<WorkItemPublisher>();
		serviceCollection.AddSingleton<IWorkItemPublisher>(sp => sp.GetRequiredService<WorkItemPublisher>());

		var builder = new Prompt2PlotBuilder(serviceCollection);
		setup(builder);
		builder.Build();

		return serviceCollection;
	}

	/// <summary>
	/// Registers the <see cref="InMemoryWorkItemRepository"/> using a settings provider.
	/// </summary>
	/// <remarks>
	/// The repository stores pending work items and completed results in memory.
	/// This overload allows repository configuration to be resolved dynamically
	/// using the dependency injection container.
	///
	/// The <paramref name="settingsProvider"/> delegate is invoked during service
	/// resolution and can use application services to construct the settings instance.
	/// </remarks>
	/// <param name="serviceCollection">
	/// The service collection used to register the repository.
	/// </param>
	/// <param name="settingsProvider">
	/// A delegate that resolves <see cref="InMemoryWorkItemRepositorySettings"/>
	/// using the service provider.
	/// </param>
	/// <returns>
	/// The same <see cref="IServiceCollection"/> instance so that additional services can be registered.
	/// </returns>
	public static IServiceCollection AddInMemoryWorkItemRepository(
		this IServiceCollection serviceCollection,
		Func<IServiceProvider, InMemoryWorkItemRepositorySettings> settingsProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentNullException.ThrowIfNull(settingsProvider);

		serviceCollection.ThrowIfRegistered<InMemoryWorkItemRepository>();

		serviceCollection.AddSingleton<InMemoryWorkItemRepository>(
			sp => new InMemoryWorkItemRepository(
				settingsProvider(sp),
				sp.GetRequiredService<IWorkItemPublisher>(),
				sp.GetService<ILoggerFactory>()));

		return serviceCollection;
	}

	/// <summary>
	/// Registers the <see cref="InMemoryWorkItemRepository"/> using the provided settings instance.
	/// </summary>
	/// <remarks>
	/// This overload is intended for scenarios where repository configuration
	/// is static and known during application startup.
	/// </remarks>
	/// <param name="serviceCollection">
	/// The service collection used to register the repository.
	/// </param>
	/// <param name="settings">
	/// The configuration used by the repository.
	/// </param>
	/// <returns>
	/// The same <see cref="IServiceCollection"/> instance so that additional services can be registered.
	/// </returns>
	public static IServiceCollection AddInMemoryWorkItemRepository(
		this IServiceCollection serviceCollection,
		InMemoryWorkItemRepositorySettings settings)
	{
		return serviceCollection.AddInMemoryWorkItemRepository(_ => settings);
	}

	/// <summary>
	/// Registers the <see cref="InMemoryWorkItemRepository"/> using default settings.
	/// </summary>
	/// <remarks>
	/// This overload provides the simplest way to enable the in-memory repository.
	/// Default <see cref="InMemoryWorkItemRepositorySettings"/> values will be used.
	/// </remarks>
	/// <param name="serviceCollection">
	/// The service collection used to register the repository.
	/// </param>
	/// <returns>
	/// The same <see cref="IServiceCollection"/> instance so that additional services can be registered.
	/// </returns>
	public static IServiceCollection AddInMemoryWorkItemRepository(this IServiceCollection serviceCollection)
	{
		return serviceCollection.AddInMemoryWorkItemRepository(_ => new InMemoryWorkItemRepositorySettings());
	}
}
