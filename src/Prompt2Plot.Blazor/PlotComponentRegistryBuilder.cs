using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot.Blazor;

public sealed class PlotComponentRegistryBuilder
{
	private readonly IServiceCollection _services;
	private readonly PlotComponentRegistry _componentRegistry = new();

	internal PlotComponentRegistryBuilder(IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		_services = services;
	}

	public PlotComponentRegistryBuilder WithComponent<TComponent>(string chartType)
		where TComponent : IComponent
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(chartType);

		_componentRegistry.RegisterComponent<TComponent>(chartType, null);

		return this;
	}

	public PlotComponentRegistryBuilder WithComponent<TComponent>(string chartType, string flowKey)
		where TComponent : IComponent
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(chartType);
		ArgumentException.ThrowIfNullOrWhiteSpace(flowKey);

		_componentRegistry.RegisterComponent<TComponent>(chartType, flowKey, null);

		return this;
	}

	public PlotComponentRegistryBuilder WithComponent<TComponent, TSettings>(string chartType, TSettings settings)
		where TComponent : IComponent
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(chartType);

		_componentRegistry.RegisterComponent<TComponent>(chartType, settings);

		return this;
	}

	public PlotComponentRegistryBuilder WithComponent<TComponent, TSettings>(
		string chartType,
		string flowKey,
		TSettings settings)
		where TComponent : IComponent
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(chartType);
		ArgumentException.ThrowIfNullOrWhiteSpace(flowKey);

		_componentRegistry.RegisterComponent<TComponent>(chartType, flowKey, settings);

		return this;
	}

	internal void Build()
	{
		_services.AddSingleton<IPlotComponentRegistry>(_componentRegistry);
	}
}
