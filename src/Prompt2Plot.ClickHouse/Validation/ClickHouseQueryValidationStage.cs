using ClickHouse.Driver;
using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse;

public sealed class ClickHouseQueryValidationStage : IValidationPipelineStage
{
	private readonly string _connectionString;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly string _httpClientName;

	public ClickHouseQueryValidationStage(
		ClickHouseQueryValidationStageSettings settings,
		ILoggerFactory? loggerFactory = null)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ArgumentNullException.ThrowIfNull(settings.ConnectionSettings);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionSettings.ConnectionString);
		ArgumentNullException.ThrowIfNull(settings.ConnectionSettings.HttpClientFactory);
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionSettings.ConnectionString);

		_connectionString = settings.ConnectionSettings.ConnectionString;
		_httpClientFactory = settings.ConnectionSettings.HttpClientFactory;
		_httpClientName = settings.ConnectionSettings.HttpClientName;
	}

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
			_connectionString,
			_httpClientFactory,
			_httpClientName);

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
