using Microsoft.Extensions.Logging;

namespace Prompt2Plot.Defaults;

public sealed class DefaultModelResponseValidator : IValidationPipelineStage
{
	public DefaultModelResponseValidator(
		DefaultModelResponseValidatorSettings settings,
		ILoggerFactory? loggerFactory = null)
	{

	}

	public Task ExecuteAsync(ValidationContext context, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(context.ModelResponse.ChartType))
		{
			context.Errors.Add("Chart type is not specified.");
			context.MarkForRetry();

			return Task.CompletedTask;
		}

		if (context.ModelResponse.Datasets == null || !context.ModelResponse.Datasets.Any())
		{
			context.Errors.Add("Failed to parse model response datasets or it is empty.");
			context.MarkForRetry();

			return Task.CompletedTask;
		}

		var queries = context.ModelResponse.Datasets!.Select(d => d.SqlQuery).ToList();

		if (queries.Any(query => query == null))
		{
			context.Errors.Add("Model response contains datasets with empty SQL queries.");
			context.MarkForRetry();

			return Task.CompletedTask;
		}

		return Task.CompletedTask;
	}
}
