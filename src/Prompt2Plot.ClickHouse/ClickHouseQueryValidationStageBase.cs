using ClickHouse.Driver;

namespace Prompt2Plot.ClickHouse;

public abstract class ClickHouseQueryValidationStageBase : IValidationPipelineStage
{
	protected abstract string ConnectionString { get; }
	protected abstract IHttpClientFactory HttpClientFactory { get; }
	protected abstract string HttpClientName { get; }

	public async Task ExecuteAsync(ValidationContext context, CancellationToken cancellationToken)
	{
		if (context.ModelResponse.Datasets == null || !context.ModelResponse.Datasets.Any())
		{
			context.Errors.Add("Failed to parse model response datasets or it is empty.");
			context.MarkForRetry();

			return;
		}

		foreach (var dataset in context.ModelResponse.Datasets)
		{
			if (dataset.SqlQuery != null)
			{
				dataset.SqlQuery = dataset.SqlQuery.Trim().TrimEnd(';');
			}
		}

		var queries = context.ModelResponse.Datasets!.Select(d => d.SqlQuery).ToList();

		if (queries.Any(query => query == null))
		{
			context.Errors.Add("Model response contains datasets with empty SQL queries.");
			context.MarkForRetry();

			return;
		}

		using var clickHouseConnection = new ClickHouseClient(
			ConnectionString,
			HttpClientFactory,
			HttpClientName);

		foreach (var query in queries)
		{
			try
			{
				await clickHouseConnection.ExecuteReaderAsync(
					$"explain plan {query}",
					cancellationToken: cancellationToken);
			}
			catch (ClickHouseServerException clickHouseException)
			{
				context.Errors.Add($"Invalid query: {query}.");
				context.MarkForRetry();

				context.RetryAuxiliaryPrompts.Add(string.Format(AuxiliaryPrompt, query, clickHouseException.Message));
			}
			catch (Exception exception)
			{
				context.Errors.Add(exception.Message);
			}
		}
	}

	private const string AuxiliaryPrompt = """
		Previously, the following ClickHouse SQL query produced an error during validation.

		Query:
		{0}

		ClickHouse error:
		{1}
		""";
}
