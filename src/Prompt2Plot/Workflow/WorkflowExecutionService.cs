using System.Collections.Concurrent;

namespace Prompt2Plot;

/// <inheritdoc/>
internal sealed class WorkflowExecutionService : IWorkflowExecutionService
{
	private readonly WorkflowFactory _workflowFactory;
	private readonly WorkItemPublisher _workItemPublisher;
	private readonly IWorkItemRepository _repository;

	// TODO : bounded through configuration
	private DeduplicatingWorkItemChannel _channel = new();

	private readonly ThreadSafeBool _isInProcess = new();
	private readonly ThreadSafeBool _isStopping = new();
	private readonly SemaphoreSlim _spinUp = new(1, 1);

	// _processed is a staging list of completed work item IDs.
	// Is effective only for background population through repository mode.
	// These are not removed from _channel._processing immediately to avoid re-enqueueing before results are persisted.
	// Items are released only during the populate cycle via PushProcessedToChannel().
	private readonly ConcurrentBag<ulong> _processed = [];

	private Task? _backgroundProcessor;
	private Task? _backgroundPopulate;
	private readonly ThreadSafeBool _populateStopRequested = new();

	private CancellationTokenSource? _cancellationTokenSource;

	public WorkflowExecutionService(
		WorkflowFactory workflowFactory,
		WorkItemPublisher publisher,
		IWorkItemRepository repository)
	{
		ArgumentNullException.ThrowIfNull(workflowFactory);
		ArgumentNullException.ThrowIfNull(publisher);
		ArgumentNullException.ThrowIfNull(repository);

		_workflowFactory = workflowFactory;
		_workItemPublisher = publisher;
		_repository = repository;
	}

	public async Task<int> ProcessPendingAsync(
		int maxDegreeOfParallelism,
		CancellationToken cancellationToken = default)
	{
		if (!_isInProcess.TrySet(true))
		{
			throw new InvalidOperationException("Workflow processing is already in progress.");
		}

		try
		{
			var workItemCount = await TryEnqueuePending(cancellationToken);

			if (workItemCount <= 0)
			{
				return 0;
			}

			await Parallel.ForEachAsync(
				Enumerable.Range(0, workItemCount),
				new ParallelOptions
				{
					MaxDegreeOfParallelism = maxDegreeOfParallelism,
					CancellationToken = cancellationToken
				},
				async (_, ct) =>
				{
					if (_channel.TryRead(out var workItem))
					{
						await TryRunWorkflow(workItem!, useDeferredProcessedHandler: false, ct);
					}
				});

			return _processed.Count;
		}
		finally
		{
			_channel = new DeduplicatingWorkItemChannel();
			_processed.Clear();

			_isInProcess.Value = false;
		}
	}

	public async Task StartAsync(int maxDegreeOfParallelism, TimeSpan populateInterval)
	{
		if (!_isInProcess.TrySet(true))
		{
			throw new InvalidOperationException("Workflow processing is already in progress.");
		}

		await _spinUp.WaitAsync();

		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;

		_backgroundPopulate ??= Task.Run(async () =>
		{
			while (!_populateStopRequested.Value && !_cancellationTokenSource.IsCancellationRequested)
			{
				AcknowledgeProcessed();
				await TryEnqueuePending(cancellationToken);
				await Task.Delay(populateInterval, cancellationToken);
			}
		}, cancellationToken);

		_backgroundProcessor ??= Parallel.ForEachAsync(
			_channel.GetConsumingEnumerable(cancellationToken),
			new ParallelOptions
			{
				MaxDegreeOfParallelism = maxDegreeOfParallelism,
				CancellationToken = cancellationToken
			},
			async (workItem, ct) => await TryRunWorkflow(workItem, useDeferredProcessedHandler: true, ct));

		_spinUp.Release();
	}

	public async Task StartAsync(int maxDegreeOfParallelism)
	{
		if (!_isInProcess.TrySet(true))
		{
			throw new InvalidOperationException("Workflow processing is already in progress.");
		}

		await _spinUp.WaitAsync();

		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;

		_workItemPublisher.Start(_channel);

		_backgroundProcessor ??= Parallel.ForEachAsync(
			_channel.GetConsumingEnumerable(cancellationToken),
			new ParallelOptions
			{
				MaxDegreeOfParallelism = maxDegreeOfParallelism,
				CancellationToken = cancellationToken
			},
			async (workItem, ct) => await TryRunWorkflow(workItem, useDeferredProcessedHandler: false, ct));

		_spinUp.Release();
	}

	public async Task StopAsync(TimeSpan timeout)
	{
		if (!_isInProcess.Value)
		{
			throw new InvalidOperationException("Workflow processing is not in progress.");
		}

		if (!_isStopping.TrySet(true))
		{
			throw new InvalidOperationException("Workflow stop is already requested.");
		}

		await _spinUp.WaitAsync();
		_spinUp.Release();

		_workItemPublisher.TryStop();
		_populateStopRequested.Value = true;
		_channel.Complete();

		if (timeout != Timeout.InfiniteTimeSpan)
		{
			_cancellationTokenSource?.CancelAfter(timeout);
		}

		var tasks = new[] { _backgroundPopulate, _backgroundProcessor }
			.Where(t => t != null)
			.Select(t => t!);

		try
		{
			await Task.WhenAll(tasks);
		}
		finally
		{
			_channel = new DeduplicatingWorkItemChannel();
			_processed.Clear();

			_populateStopRequested.Value = false;
			_backgroundPopulate = null;
			_backgroundProcessor = null;

			_isInProcess.Value = false;
			_isStopping.Value = false;

			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}
	}

	private void AcknowledgeProcessed()
	{
		while (_processed.TryTake(out var id))
		{
			_channel.TryAcknowledge(id);
		}
	}

	private async Task<int> TryEnqueuePending(CancellationToken cancellationToken)
	{
		try
		{
			var items = await _repository.GetPendingWorkItemsAsync(cancellationToken);

			return items.Count(item => _channel.TryWrite(item));
		}
		catch
		{
			return 0;
		}
	}

	private async Task TryRunWorkflow(
		WorkItem workItem,
		bool useDeferredProcessedHandler,
		CancellationToken cancellationToken)
	{
		try
		{
			var workflow = _workflowFactory.GetWorkflow(workItem.WorkflowKey);
			var result = await workflow.RunAsync(workItem, cancellationToken);

			// TODO:
			// if result saving throws we will try to execute saving again,
			// which might lead to two results for single work item
			await SaveResult(result, cancellationToken);
		}
		catch (Exception exception)
		{
			var errorResult = new WorkItemResult
			{
				WorkItemId = workItem.Id,
				Success = false,
				Errors = [exception.Message],
			};

			await TrySaveResult(errorResult, cancellationToken);
		}
		finally
		{
			if (useDeferredProcessedHandler)
			{
				_processed.Add(workItem.Id);
			}
			else
			{
				_channel.TryAcknowledge(workItem.Id);
			}
		}
	}

	private async Task<bool> TrySaveResult(WorkItemResult workItemResult, CancellationToken cancellationToken)
	{
		try
		{
			await SaveResult(workItemResult, cancellationToken);

			return true;
		}
		catch
		{
			return false;
		}
	}

	private async Task SaveResult(WorkItemResult workItemResult, CancellationToken cancellationToken)
	{
		await _repository.AddWorkItemResultAsync(workItemResult, cancellationToken);
	}
}
