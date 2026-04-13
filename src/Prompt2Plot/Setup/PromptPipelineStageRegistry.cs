using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

internal sealed class PromptPipelineStageRegistry
{
	private readonly HashSet<Type> _stageTypes = [];
	private readonly List<Func<IServiceProvider, object?, IPromptPipelineStage>> _factories = [];

	public void AddStage<TStage>(Func<IServiceProvider, object?, TStage> factory)
		where TStage : class, IPromptPipelineStage
	{
		if (!_stageTypes.Add(typeof(TStage)))
		{
			throw new InvalidOperationException($"Stage of type {typeof(TStage)} is already registered.");
		}

		_factories.Add(factory);
	}

	public void AddStage<TStage>()
		where TStage : class, IPromptPipelineStage
	{
		AddStage<TStage>((sp, key) => sp.GetRequiredKeyedService<TStage>(key));
	}

	public bool Any()
	{
		return _stageTypes.Count != 0;
	}

	public IEnumerable<IPromptPipelineStage> GetStages(IServiceProvider serviceProvider, object? key)
	{
		return _factories.Select(factory => factory(serviceProvider, key));
	}

	public void RegisterStages(IServiceCollection serviceCollection, object? key)
	{
		foreach (var stage in _stageTypes)
		{
			serviceCollection.AddKeyedSingleton(stage, key);
		}
	}
}
