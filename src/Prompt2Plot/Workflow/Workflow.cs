using System.Collections.Concurrent;

namespace Prompt2Plot;

internal sealed class Workflow
{
	private readonly PromptPipeline _promptPipeline;
	private readonly IPromptExecutor _promptExecutor;

	private readonly ValidationPipeline? _validationPipeline;
	private readonly ISqlQueryExecutor _sqlQueryExecutor;

	public Workflow(
		PromptPipeline promptPipeline,
		IPromptExecutor promptExecutor,
		ValidationPipeline? validationPipeline,
		ISqlQueryExecutor sqlQueryExecutor)
	{
		ArgumentNullException.ThrowIfNull(promptPipeline);
		ArgumentNullException.ThrowIfNull(promptExecutor);
		ArgumentNullException.ThrowIfNull(sqlQueryExecutor);

		_promptPipeline = promptPipeline;
		_promptExecutor = promptExecutor;
		_validationPipeline = validationPipeline;
		_sqlQueryExecutor = sqlQueryExecutor;
	}

	public Task<WorkItemResult> RunAsync(WorkItem workItem, CancellationToken cancellationToken)
	{
		return RunAsync(workItem, retryAttempt: 0, promptOverride: null, auxiliaryPrompts: [], cancellationToken);
	}

	private async Task<WorkItemResult> RunAsync(
		WorkItem workItem,
		int retryAttempt,
		string? promptOverride,
		List<string> auxiliaryPrompts,
		CancellationToken cancellationToken)
	{
		var (promptContext, errorResult) = await GeneratePromptAsync(
			workItem,
			promptOverride,
			auxiliaryPrompts,
			cancellationToken);

		if (errorResult != null)
		{
			return errorResult;
		}

		// TODO : prompt execution context; validate llm errors
		var modelResponse = await _promptExecutor.ExecuteAsync(promptContext, cancellationToken);

		var validationContext = new ValidationContext
		{
			WorkItemId = workItem.Id,
			NaturalLanguageRequest = workItem.NaturalLanguageRequest,
			Prompt = promptContext.Prompt,
			ModelResponse = modelResponse,
		};

		if (_validationPipeline != null)
		{
			validationContext = await _validationPipeline.RunAsync(validationContext, cancellationToken);

			if (validationContext.ShouldRetry)
			{
				if (retryAttempt < _validationPipeline.MaxRetries)
				{
					return await RunAsync(
						workItem,
						retryAttempt + 1,
						promptContext.Prompt,
						validationContext.RetryAuxiliaryPrompts,
						cancellationToken);
				}

				var validationsErrors = new List<string> { "Workflow generated validation errors." };
				validationsErrors.AddRange(validationContext.Errors);

				return new WorkItemResult
				{
					WorkItemId = workItem.Id,
					WorkflowKey = workItem.WorkflowKey,
					Success = false,
					Errors = validationsErrors,
				};
			}
		}

		if (validationContext.ModelResponse.Datasets == null || !validationContext.ModelResponse.Datasets.Any())
		{
			return new WorkItemResult
			{
				WorkItemId = workItem.Id,
				WorkflowKey = workItem.WorkflowKey,
				Success = false,
				Errors = ["Failed to parse model response datasets or it is empty."]
			};
		}

		if (validationContext.ModelResponse.Datasets!.Any(ds => string.IsNullOrWhiteSpace(ds.SqlQuery)))
		{
			return new WorkItemResult
			{
				WorkItemId = workItem.Id,
				WorkflowKey = workItem.WorkflowKey,
				Success = false,
				Errors = ["Model response contains datasets with empty SQL queries."]
			};
		}

		var dbResponses = new ConcurrentBag<(DatabaseResponse DbResponse, ModelResponseDataset ModelResponse)>();

		await Parallel.ForEachAsync(
			validationContext.ModelResponse.Datasets!,
			new ParallelOptions
			{
				CancellationToken = cancellationToken,
				MaxDegreeOfParallelism = _sqlQueryExecutor.MaxParallelQueries,
			},
			async (dataset, ct) =>
			{
				var dbResponse = await _sqlQueryExecutor.ExecuteAsync(dataset.SqlQuery!, ct);
				dbResponses.Add((dbResponse, dataset));
			});

		var resultDatasets = dbResponses
			.Select(r => new WorkItemResultDataset
			{
				SqlQuery = r.ModelResponse.SqlQuery,
				Label = r.ModelResponse.Label,
				Fields = r.DbResponse.Fields,
				Rows = r.DbResponse.Rows,
				Error = r.DbResponse.Error,
			})
			.ToList();

		var datasetExecutionErrors = resultDatasets
			.Where(d => !string.IsNullOrEmpty(d.Error))
			.Select(d => d.Error)
			.ToList();

		var success = datasetExecutionErrors.Count == 0;

		var errors = success ? [] : new List<string> { "Dataset execution errors occured." };
		errors.AddRange(datasetExecutionErrors!);

		return new WorkItemResult
		{
			WorkItemId = workItem.Id,
			WorkflowKey = workItem.WorkflowKey,
			Success = success,
			ChartType = validationContext.ModelResponse.ChartType,
			ChartDescription = validationContext.ModelResponse.ChartDescription,
			Datasets = resultDatasets,
			Errors = errors,
		};
	}

	private async Task<(PromptContext Context, WorkItemResult? ErrorResult)> GeneratePromptAsync(
		WorkItem workItem,
		string? promptOverride,
		List<string> auxiliaryPrompts,
		CancellationToken cancellationToken)
	{
		var promptContext = new PromptContext
		{
			WorkItemId = workItem.Id,
			NaturalLanguageRequest = workItem.NaturalLanguageRequest,
		};

		if (string.IsNullOrEmpty(promptOverride))
		{
			await _promptPipeline.RunAsync(promptContext, cancellationToken);
		}
		else
		{
			promptContext.Prompt = promptOverride;
		}

		if (promptContext.Errors.Count != 0)
		{
			var promptGenerationErrors = new List<string> { "Prompt generation errors occured." };
			promptGenerationErrors.AddRange(promptContext.Errors);

			return (promptContext, new WorkItemResult
			{
				WorkItemId = workItem.Id,
				WorkflowKey = workItem.WorkflowKey,
				Success = false,
				Errors = promptGenerationErrors,
			});
		}

		if (auxiliaryPrompts.Count != 0)
		{
			promptContext.Prompt += string.Join("\n", auxiliaryPrompts);
		}

		return (promptContext, null);
	}
}
