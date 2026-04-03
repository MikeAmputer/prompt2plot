namespace Prompt2Plot;

/// <summary>
/// Coordinates the execution of workflows using a background processing pipeline.
/// Supports both single-pass (batch) execution and continuous background processing
/// with periodic population from a repository.
/// </summary>
/// <remarks>
/// This service maintains internal state to prevent duplicate processing of the same
/// work item concurrently, and supports graceful shutdown with optional timeouts.
/// </remarks>
public interface IWorkflowExecutionService
{
	/// <summary>
	/// Processes all currently pending work items from <see cref="IWorkItemRepository"/> in a single batch.
	/// </summary>
	///
	/// <returns>The number of successfully processed work items.</returns>
	///
	/// <param name="maxDegreeOfParallelism">The maximum number of workflows to run concurrently.</param>
	/// <param name="cancellationToken">
	/// A token that can be used to cancel workflow execution.
	/// If cancelled, the method throws <see cref="OperationCanceledException"/> or <see cref="TaskCanceledException"/>.
	/// </param>
	///
	/// <exception cref="InvalidOperationException">Workflow processing is already in progress.</exception>
	Task<int> ProcessPendingAsync(int maxDegreeOfParallelism, CancellationToken cancellationToken = default);

	/// <summary>
	/// Start consuming work items and executing workflows. Population will be executed through <see cref="IWorkItemRepository"/>.
	/// </summary>
	///
	/// <param name="maxDegreeOfParallelism">The maximum number of workflows to run concurrently.</param>
	/// <param name="populateInterval">Interval of work item population through <see cref="IWorkItemRepository"/>.</param>
	///
	/// <exception cref="InvalidOperationException">Workflow processing is already in progress.</exception>
	Task StartAsync(int maxDegreeOfParallelism, TimeSpan populateInterval);

	/// <summary>
	/// Start consuming work items and executing workflows. Population will be executed through <see cref="IWorkItemPublisher"/>.
	/// </summary>
	///
	/// <param name="maxDegreeOfParallelism">The maximum number of workflows to run concurrently.</param>
	///
	/// <exception cref="InvalidOperationException">Workflow processing is already in progress.</exception>
	Task StartAsync(int maxDegreeOfParallelism);

	/// <summary>
	/// Gracefully stop work items population and consumption.
	/// </summary>
	///
	/// <param name="timeout">Time until cancellation will be forced.</param>
	///
	/// <exception cref="InvalidOperationException">Workflow processing is not in progress or stop is already requested.</exception>
	Task StopAsync(TimeSpan timeout);
}
