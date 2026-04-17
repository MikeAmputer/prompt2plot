using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot;

namespace BlazorDemo.Services;

public class WorkflowHostedService : IHostedService
{
	private readonly IWorkflowExecutionService _executor;
	private readonly ILogger _logger;

	public WorkflowHostedService(IWorkflowExecutionService executor, ILogger<WorkflowHostedService>? logger = null)
	{
		_executor = executor;
		_logger = logger ?? NullLogger<WorkflowHostedService>.Instance;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Starting hosted service...");

		await _executor.StartAsync(1);

		_logger.LogInformation("Hosted service is started.");
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping hosted service...");

		await _executor.StopAsync(TimeSpan.FromSeconds(120));

		_logger.LogInformation("Hosted service is stopped.");
	}
}
