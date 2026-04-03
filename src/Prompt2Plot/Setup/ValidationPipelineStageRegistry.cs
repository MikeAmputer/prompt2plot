using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Prompt2Plot;

internal sealed class ValidationPipelineStageRegistry
{
	private readonly HashSet<Type> _stages = [];
	private readonly List<Type> _orderedStages = [];

	public void AddStage<TStage>()
		where TStage : class, IValidationPipelineStage
	{
		if (!_stages.Add(typeof(TStage)))
		{
			throw new InvalidOperationException($"Validation stage of type {typeof(TStage)} is already registered.");
		}

		_orderedStages.Add(typeof(TStage));
	}

	public bool Any()
	{
		return _stages.Count != 0;
	}

	public IEnumerable<IValidationPipelineStage> GetStages(IServiceProvider serviceProvider, object? key)
	{
		return _orderedStages.Select(t =>
		{
			var service = serviceProvider.GetRequiredKeyedService(t, key);

			return (IValidationPipelineStage) service;
		});
	}

	public void RegisterStages(IServiceCollection serviceCollection, object? key)
	{
		foreach (var stage in _stages)
		{
			serviceCollection.AddKeyedSingleton(stage, key);
		}
	}
}
