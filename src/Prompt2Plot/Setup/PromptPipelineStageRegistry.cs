using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Prompt2Plot;

internal sealed class PromptPipelineStageRegistry
{
	private readonly HashSet<Type> _stages = [];
	private readonly List<Type> _orderedStages = [];

	public void AddStage<TStage>()
		where TStage : class, IPromptPipelineStage
	{
		if (!_stages.Add(typeof(TStage)))
		{
			throw new InvalidOperationException($"Stage of type {typeof(TStage)} is already registered.");
		}

		_orderedStages.Add(typeof(TStage));
	}

	public bool Any()
	{
		return _stages.Count != 0;
	}

	public IEnumerable<IPromptPipelineStage> GetStages(IServiceProvider serviceProvider, object? key)
	{
		return _orderedStages.Select(t =>
		{
			var service = serviceProvider.GetRequiredKeyedService(t, key);

			return (IPromptPipelineStage) service;
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
