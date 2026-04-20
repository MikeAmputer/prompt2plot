using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddClickHouseQueryExecutor(
		this IServiceCollection serviceCollection,
		string flowKey,
		Func<IServiceProvider, ClickHouseQueryExecutorSettings> settingsProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentException.ThrowIfNullOrWhiteSpace(flowKey);
		ArgumentNullException.ThrowIfNull(settingsProvider);

		serviceCollection.ThrowIfRegistered<ClickHouseQueryExecutor>(flowKey);

		serviceCollection.AddKeyedSingleton<ClickHouseQueryExecutor>(
			flowKey,
			(sp, _) => new ClickHouseQueryExecutor(
				settingsProvider(sp),
				sp.GetService<ILoggerFactory>()));

		return serviceCollection;
	}

	public static IServiceCollection AddClickHouseQueryExecutor(
		this IServiceCollection serviceCollection,
		string flowKey,
		Func<IServiceProvider, ClickHouseConnectionSettings> settingsProvider)
	{
		return serviceCollection.AddClickHouseQueryExecutor(
			flowKey,
			sp => new ClickHouseQueryExecutorSettings { ConnectionSettings = settingsProvider(sp) });
	}
}
