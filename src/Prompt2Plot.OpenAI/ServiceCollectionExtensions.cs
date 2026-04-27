using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Prompt2Plot.OpenAI;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGptStructuredPromptExecutor(
		this IServiceCollection serviceCollection,
		string flowKey,
		Func<IServiceProvider, object?, GptPromptExecutorSettings> settingsProvider,
		ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentException.ThrowIfNullOrWhiteSpace(flowKey);
		ArgumentNullException.ThrowIfNull(settingsProvider);

		serviceCollection.ThrowIfRegistered<GptStructuredPromptExecutor>(flowKey);

		var descriptor = new ServiceDescriptor(
			serviceType: typeof(GptStructuredPromptExecutor),
			serviceKey: flowKey,
			factory: (sp, key) => new GptStructuredPromptExecutor(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()),
			lifetime: serviceLifetime);

		serviceCollection.Add(descriptor);

		return serviceCollection;
	}

	public static IServiceCollection AddGptStructuredPromptExecutor(
		this IServiceCollection services,
		string flowKey,
		Func<IServiceProvider, GptPromptExecutorSettings> settingsProvider,
		ServiceLifetime lifetime = ServiceLifetime.Singleton)
	{
		return services.AddGptStructuredPromptExecutor(
			flowKey,
			(sp, _) => settingsProvider(sp),
			lifetime);
	}
}
