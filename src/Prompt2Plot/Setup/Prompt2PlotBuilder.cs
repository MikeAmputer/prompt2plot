using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

public sealed class Prompt2PlotBuilder
{
	private OptionalValue<Func<IServiceProvider, IWorkItemRepository>> _workItemRepositoryFactory;
	private readonly Dictionary<string, Action<WorkflowServiceBuilder>> _workflows = [];

	internal Prompt2PlotBuilder() { }

	public Prompt2PlotBuilder WithWorkItemRepository<TRepository>()
		where TRepository : class, IWorkItemRepository
	{
		_workItemRepositoryFactory.ThrowIfSet(nameof(_workItemRepositoryFactory));

		return WithWorkItemRepository(sp => sp.GetRequiredService<TRepository>());
	}

	public Prompt2PlotBuilder WithWorkItemRepository(Func<IServiceProvider, IWorkItemRepository> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);

		_workItemRepositoryFactory.ThrowIfSet(nameof(_workItemRepositoryFactory));
		_workItemRepositoryFactory = factory;

		return this;
	}

	public Prompt2PlotBuilder AddWorkflow(string name, Action<WorkflowServiceBuilder> setup)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(setup);

		if (!_workflows.TryAdd(name, setup))
		{
			throw new InvalidOperationException($"Workflow '{name}' already registered.");
		}

		return this;
	}

	public Prompt2PlotBuilder AddWorkflow<TPromptExecutor, TSqlExecutor>(
		string name,
		Action<IStrictWorkflowServiceBuilder> setup)
		where TPromptExecutor : class, IPromptExecutor
		where TSqlExecutor : class, ISqlQueryExecutor
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(setup);

		if (!_workflows.TryAdd(name, Builder))
		{
			throw new InvalidOperationException($"Workflow '{name}' already registered.");
		}

		return this;

		void Builder(WorkflowServiceBuilder builder)
		{
			setup(builder);

			builder
				.WithPromptExecutor(serviceProvider =>
					serviceProvider.GetRequiredKeyedService<TPromptExecutor>(name))
				.WithSqlQueryExecutor(serviceProvider =>
					serviceProvider.GetRequiredKeyedService<TSqlExecutor>(name));
		}
	}

	internal void Build(IServiceCollection serviceCollection)
	{
		serviceCollection.ThrowIfRegistered<IWorkItemRepository>();

		var workItemRepositoryFactory = _workItemRepositoryFactory
			.NotNullOrThrow(nameof(_workItemRepositoryFactory));
		serviceCollection.AddTransient(workItemRepositoryFactory);

		foreach (var (key, workflowSetup) in _workflows)
		{
			var workflowServiceBuilder = new WorkflowServiceBuilder(key);
			workflowSetup(workflowServiceBuilder);
			workflowServiceBuilder.Build(serviceCollection);
		}
	}
}
