using Microsoft.AspNetCore.Components;

namespace Prompt2Plot.Blazor;

internal sealed class PlotComponentRegistry : IPlotComponentRegistry
{
	private readonly Dictionary<string, PlotComponentRegistration> _components =
		new(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<(string Chart, string Flow), PlotComponentRegistration> _flowSpecificComponents =
		new(new KeyComparer());

	internal void RegisterComponent<T>(string chartType, object? settings)
		where T : IComponent
	{
		if (!_components.TryAdd(chartType, new(typeof(T), settings)))
		{
			throw new InvalidOperationException($"Component for chart type '{chartType}' is already registered.");
		}
	}

	internal void RegisterComponent<T>(string chartType, string flowKey, object? settings)
		where T : IComponent
	{
		if (!_flowSpecificComponents.TryAdd((chartType, flowKey), new(typeof(T), settings)))
		{
			throw new InvalidOperationException(
				$"Component for chart type '{chartType}' of '{flowKey}' workflow is already registered.");
		}
	}

	public PlotComponentRegistration? Resolve(string chartType, string flowKey)
	{
		return _flowSpecificComponents.GetValueOrDefault((chartType, flowKey))
		       ?? _components.GetValueOrDefault(chartType);
	}

	private sealed class KeyComparer : IEqualityComparer<(string Chart, string Flow)>
	{
		private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

		public bool Equals((string Chart, string Flow) x, (string Chart, string Flow) y)
			=> Comparer.Equals(x.Chart, y.Chart) && Comparer.Equals(x.Flow, y.Flow);

		public int GetHashCode((string Chart, string Flow) obj)
			=> HashCode.Combine(
				Comparer.GetHashCode(obj.Chart),
				Comparer.GetHashCode(obj.Flow));
	}
}
