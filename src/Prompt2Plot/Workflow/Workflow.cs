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
		return RunAsync(workItem, retryAttempt: 0, string.Empty, cancellationToken);
	}

	private async Task<WorkItemResult> RunAsync(
		WorkItem workItem,
		int retryAttempt,
		string auxiliaryPrompt,
		CancellationToken cancellationToken)
	{
		var (promptContext, errorResult) = await GeneratePromptAsync(workItem, auxiliaryPrompt, cancellationToken);

		if (errorResult != null)
		{
			return errorResult;
		}

		var modelResponse = await _promptExecutor.ExecuteAsync(promptContext, cancellationToken);

		var validationContext = new ValidationContext
		{
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
					await RunAsync(
						workItem,
						retryAttempt + 1,
						validationContext.RetryAuxiliaryPrompt,
						cancellationToken);
				}
				else
				{
					return new WorkItemResult
					{
						WorkItemId = workItem.Id,
						Success = false,
						Error = validationContext.Error == null
							? "Validation error."
							: $"Validation error: {validationContext.Error}"
					};
				}
			}
		}

		if (validationContext.ModelResponse.Datasets == null)
		{
			return new WorkItemResult
			{
				WorkItemId = workItem.Id,
				Success = false,
				Error = "Failed to parse model response datasets."
			};
		}

		if (validationContext.ModelResponse.Datasets.Any(ds => string.IsNullOrWhiteSpace(ds.SqlQuery)))
		{
			return new WorkItemResult
			{
				WorkItemId = workItem.Id,
				Success = false,
				Error = "Model response contains datasets with empty SQL queries."
			};
		}

		var dbResponses = new ConcurrentBag<(DatabaseResponse DbResponse, ModelResponseDataset ModelResponse)>();

		await Parallel.ForEachAsync(
			validationContext.ModelResponse.Datasets,
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
				Data = r.DbResponse.Data,
				Error = r.DbResponse.Error,
			})
			.ToList();

		var errors = resultDatasets
			.Where(d => !string.IsNullOrEmpty(d.Error))
			.Select(d => d.Error)
			.ToList();

		var aggregatedError = errors.Count != 0 ? string.Join("; ", errors) : null;
		var success = errors.Count == 0;

		return new WorkItemResult
		{
			WorkItemId = workItem.Id,
			Success = success,
			ChartType = validationContext.ModelResponse.ChartType,
			ChartDescription = validationContext.ModelResponse.ChartDescription,
			Datasets = resultDatasets,
			Error = aggregatedError,
		};
	}

	private async Task<(PromptContext Context, WorkItemResult? ErrorResult)> GeneratePromptAsync(
		WorkItem workItem,
		string auxiliaryPrompt,
		CancellationToken cancellationToken)
	{
		var promptContext = new PromptContext
		{
			NaturalLanguageRequest = workItem.NaturalLanguageRequest
		};

		await _promptPipeline.RunAsync(promptContext, cancellationToken);

		if (promptContext.Error != null)
		{
			return (promptContext, new WorkItemResult
			{
				WorkItemId = workItem.Id,
				Success = false,
				Error = $"Prompt generation error: {promptContext.Error}"
			});
		}

		if (!string.IsNullOrWhiteSpace(auxiliaryPrompt))
		{
			promptContext.Prompt += auxiliaryPrompt;
		}

		return (promptContext, null);
	}
}
