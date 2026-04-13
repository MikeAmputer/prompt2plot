using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Prompt2Plot;

internal sealed class ValidationPipelineStageRegistry
{
	private readonly HashSet<Type> _stageTypes = [];
	private readonly List<Func<IServiceProvider, object?, IValidationPipelineStage>> _factories = [];

	public void AddStage<TStage>(Func<IServiceProvider, object?, TStage> factory)
		where TStage : class, IValidationPipelineStage
	{
		if (!_stageTypes.Add(typeof(TStage)))
		{
			throw new InvalidOperationException($"Validation stage of type {typeof(TStage)} is already registered.");
		}

		_factories.Add(factory);
	}

	public void AddStage<TStage>()
		where TStage : class, IValidationPipelineStage
	{
		AddStage<TStage>((sp, key) => sp.GetRequiredKeyedService<TStage>(key));
	}

	public bool Any()
	{
		return _stageTypes.Count != 0;
	}

	public IEnumerable<IValidationPipelineStage> GetStages(IServiceProvider serviceProvider, object? key)
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
