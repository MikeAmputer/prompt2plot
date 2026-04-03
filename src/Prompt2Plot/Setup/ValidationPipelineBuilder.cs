using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

public sealed class ValidationPipelineBuilder
{
	private readonly ValidationPipelineStageRegistry _stageRegistry = new();
	private OptionalValue<int> _maxRetries;

	public ValidationPipelineBuilder AddStage<TStage>()
		where TStage : class, IValidationPipelineStage
	{
		_stageRegistry.AddStage<TStage>();

		return this;
	}

	public ValidationPipelineBuilder WithMaxRetries(int maxRetries)
	{
		if (maxRetries < 0)
		{
			throw new ArgumentException("Value should be not less than zero.", nameof(maxRetries));
		}

		_maxRetries.ThrowIfSet(nameof(_maxRetries));
		_maxRetries = maxRetries;

		return this;
	}

	internal void Build(IServiceCollection serviceCollection, string key)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		if (!_stageRegistry.Any())
		{
			throw new InvalidOperationException($"No validation pipeline stages for workflow '{key}'.");
		}

		serviceCollection.AddKeyedSingleton(key, _stageRegistry);
		_stageRegistry.RegisterStages(serviceCollection, key);

		var maxRetries = _maxRetries.OrElseValue(0);

		serviceCollection.AddKeyedTransient<ValidationPipeline>(key, (serviceProvider, serviceKey) =>
		{
			var stageRegistry =
				serviceProvider.GetRequiredKeyedService<ValidationPipelineStageRegistry>(serviceKey);
			var stages = stageRegistry.GetStages(serviceProvider, serviceKey).ToList();

			return new ValidationPipeline(stages)
			{
				MaxRetries = maxRetries,
			};
		});
	}
}
