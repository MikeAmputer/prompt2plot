using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

internal static class ServiceCollectionGuards
{
	public static void ThrowIfRegistered<T>(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		if (services.Any(d => d.ServiceType == typeof(T)))
		{
			throw new InvalidOperationException($"Service '{typeof(T).Name}' is already registered.");
		}
	}

	public static void ThrowIfRegistered<T>(this IServiceCollection services, object? serviceKey)
	{
		ArgumentNullException.ThrowIfNull(services);

		if (services.Any(d => d.ServiceType == typeof(T) && Equals(d.ServiceKey, serviceKey)))
		{
			throw new InvalidOperationException(
				$"Service '{typeof(T).Name}' with key '{serviceKey}' is already registered.");
		}
	}
}
