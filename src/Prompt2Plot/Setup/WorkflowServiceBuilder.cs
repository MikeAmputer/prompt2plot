using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

public sealed class WorkflowServiceBuilder : IStrictWorkflowServiceBuilder
{
	private OptionalValue<Action<PromptPipelineBuilder>> _promptPipelineBuilderSetup;
	private OptionalValue<Action<ValidationPipelineBuilder>> _validationPipelineBuilderSetup;
	private OptionalValue<Func<IServiceProvider, object?, IPromptExecutor>> _promptExecutorFactory;
	private OptionalValue<Func<IServiceProvider, object?, ISqlQueryExecutor>> _sqlQueryExecutorFactory;

	public WorkflowServiceBuilder WithPromptPipeline(Action<PromptPipelineBuilder> setup)
	{
		ArgumentNullException.ThrowIfNull(setup);

		_promptPipelineBuilderSetup.ThrowIfSet(nameof(_promptPipelineBuilderSetup));
		_promptPipelineBuilderSetup = setup;

		return this;
	}

	IStrictWorkflowServiceBuilder IStrictWorkflowServiceBuilder.WithPromptPipeline(
		Action<PromptPipelineBuilder> setup)
	{
		return WithPromptPipeline(setup);
	}

	public WorkflowServiceBuilder WithValidationPipeline(Action<ValidationPipelineBuilder> setup)
	{
		ArgumentNullException.ThrowIfNull(setup);

		_validationPipelineBuilderSetup.ThrowIfSet(nameof(_validationPipelineBuilderSetup));
		_validationPipelineBuilderSetup = setup;

		return this;
	}

	IStrictWorkflowServiceBuilder IStrictWorkflowServiceBuilder.WithValidationPipeline(
		Action<ValidationPipelineBuilder> setup)
	{
		return WithValidationPipeline(setup);
	}

	public WorkflowServiceBuilder WithPromptExecutor<T>()
		where T : IPromptExecutor
	{
		_promptExecutorFactory.ThrowIfSet(nameof(_promptExecutorFactory));
		_promptExecutorFactory = new OptionalValue<Func<IServiceProvider, object?, IPromptExecutor>>(
			(sp, key) => sp.GetRequiredKeyedService<T>(key));

		return this;
	}

	public WorkflowServiceBuilder WithPromptExecutor(Func<IServiceProvider, IPromptExecutor> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);

		_promptExecutorFactory.ThrowIfSet(nameof(_promptExecutorFactory));
		_promptExecutorFactory = new OptionalValue<Func<IServiceProvider, object?, IPromptExecutor>>(
			(sp, _) => factory(sp));

		return this;
	}

	public WorkflowServiceBuilder WithPromptExecutor(Func<IServiceProvider, object?, IPromptExecutor> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);

		_promptExecutorFactory.ThrowIfSet(nameof(_promptExecutorFactory));
		_promptExecutorFactory = factory;

		return this;
	}

	public WorkflowServiceBuilder WithSqlQueryExecutor<T>()
		where T : ISqlQueryExecutor
	{
		_sqlQueryExecutorFactory.ThrowIfSet(nameof(_sqlQueryExecutorFactory));
		_sqlQueryExecutorFactory = new OptionalValue<Func<IServiceProvider, object?, ISqlQueryExecutor>>(
				(sp, key) => sp.GetRequiredKeyedService<T>(key));

		return this;
	}

	public WorkflowServiceBuilder WithSqlQueryExecutor(Func<IServiceProvider, ISqlQueryExecutor> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);

		_sqlQueryExecutorFactory.ThrowIfSet(nameof(_sqlQueryExecutorFactory));
		_sqlQueryExecutorFactory = new OptionalValue<Func<IServiceProvider, object?, ISqlQueryExecutor>>(
			(sp, _) => factory(sp));

		return this;
	}

	public WorkflowServiceBuilder WithSqlQueryExecutor(Func<IServiceProvider, object?, ISqlQueryExecutor> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);

		_sqlQueryExecutorFactory.ThrowIfSet(nameof(_sqlQueryExecutorFactory));
		_sqlQueryExecutorFactory = factory;

		return this;
	}

	internal void Build(IServiceCollection serviceCollection, string key)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		var promptPipelineBuilder = new PromptPipelineBuilder();
		_promptPipelineBuilderSetup
			.NotNullOrThrow(nameof(_promptPipelineBuilderSetup))
			.Invoke(promptPipelineBuilder);

		promptPipelineBuilder.Build(serviceCollection, key);

		if (_validationPipelineBuilderSetup.HasValue)
		{
			var validationPipelineBuilder = new ValidationPipelineBuilder();
			_validationPipelineBuilderSetup
				.NotNullOrThrow(nameof(_validationPipelineBuilderSetup))
				.Invoke(validationPipelineBuilder);

			validationPipelineBuilder.Build(serviceCollection, key);
		}

		var promptExecutorFactory = _promptExecutorFactory.NotNullOrThrow(nameof(_promptExecutorFactory));
		serviceCollection.AddKeyedTransient(key, promptExecutorFactory);

		var sqlQueryExecutorFactory = _sqlQueryExecutorFactory.NotNullOrThrow(nameof(_sqlQueryExecutorFactory));
		serviceCollection.AddKeyedTransient(key, sqlQueryExecutorFactory);
	}
}
