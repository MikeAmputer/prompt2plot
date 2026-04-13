using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

public sealed class PromptPipelineBuilder
{
	private readonly PromptPipelineStageRegistry _stageRegistry = new();

	internal PromptPipelineBuilder() { }

	public PromptPipelineBuilder AddStage<TStage>()
		where TStage : class, IPromptPipelineStage
	{
		_stageRegistry.AddStage<TStage>();

		return this;
	}

	public PromptPipelineBuilder AddStage<TStage>(Func<IServiceProvider, object?, TStage> factory)
		where TStage : class, IPromptPipelineStage
	{
		_stageRegistry.AddStage(factory);

		return this;
	}

	internal void Build(IServiceCollection serviceCollection, string key)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		if (!_stageRegistry.Any())
		{
			throw new InvalidOperationException($"No prompt pipeline stages for workflow '{key}'.");
		}

		serviceCollection.AddKeyedSingleton(key, _stageRegistry);

		_stageRegistry.RegisterStages(serviceCollection, key);

		serviceCollection.AddKeyedTransient<PromptPipeline>(key, (serviceProvider, serviceKey) =>
		{
			var stageRegistry = serviceProvider.GetRequiredKeyedService<PromptPipelineStageRegistry>(serviceKey);
			var stages = stageRegistry.GetStages(serviceProvider, serviceKey).ToList();

			return new PromptPipeline(stages);
		});
	}
}
