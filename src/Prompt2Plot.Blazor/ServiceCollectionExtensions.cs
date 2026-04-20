using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot.Blazor;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPlotRendering(
		this IServiceCollection services,
		Action<PlotComponentRegistryBuilder> setup)
	{
		if (services.Any(d => d.ServiceType == typeof(IPlotComponentRegistry)))
		{
			throw new InvalidOperationException($"Service '{nameof(IPlotComponentRegistry)}' is already registered.");
		}

		var builder = new PlotComponentRegistryBuilder(services);
		setup(builder);
		builder.Build();

		return services;
	}
}
