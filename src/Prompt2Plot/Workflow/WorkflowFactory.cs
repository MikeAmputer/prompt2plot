using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot;

internal sealed class WorkflowFactory
{
	private readonly IKeyedServiceProvider _serviceProvider;

	public WorkflowFactory(IServiceProvider serviceProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceProvider);

		_serviceProvider = (IKeyedServiceProvider) serviceProvider;
	}

	public Workflow GetWorkflow(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return new Workflow(
			_serviceProvider.GetRequiredKeyedService<PromptPipeline>(name),
			_serviceProvider.GetRequiredKeyedService<IPromptExecutor>(name),
			_serviceProvider.GetKeyedService<ValidationPipeline>(name),
			_serviceProvider.GetRequiredKeyedService<ISqlQueryExecutor>(name)
		);
	}
}
