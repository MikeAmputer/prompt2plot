using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddClickHouseQueryExecutor(
		this IServiceCollection serviceCollection,
		string flowName,
		Func<IServiceProvider, ClickHouseQueryExecutorSettings> settingsProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentException.ThrowIfNullOrWhiteSpace(flowName);
		ArgumentNullException.ThrowIfNull(settingsProvider);

		serviceCollection.AddKeyedSingleton<ClickHouseQueryExecutor>(
			flowName,
			(sp, _) => new ClickHouseQueryExecutor(
				settingsProvider(sp),
				sp.GetService<ILoggerFactory>()));

		return serviceCollection;
	}

	public static IServiceCollection AddClickHouseQueryExecutor(
		this IServiceCollection serviceCollection,
		string flowName,
		Func<IServiceProvider, ClickHouseConnectionSettings> settingsProvider)
	{
		return serviceCollection.AddClickHouseQueryExecutor(
			flowName,
			sp => new ClickHouseQueryExecutorSettings { ConnectionSettings = settingsProvider(sp) });
	}
}
