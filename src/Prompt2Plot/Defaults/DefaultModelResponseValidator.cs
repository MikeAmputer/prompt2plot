using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.Logging;

namespace Prompt2Plot.Defaults;

/// <summary>
/// Validates the structured response produced by the language model before database-specific validation is performed.
/// </summary>
/// <remarks>
/// This stage performs lightweight validation of the model output
/// to ensure that the response structure is complete and safe to process.
///
/// The following checks are performed:
///
/// <list type="bullet">
/// <item><description>
/// Ensures that a chart type is specified.
/// </description></item>
/// <item><description>
/// Ensures that at least one dataset is present.
/// </description></item>
/// <item><description>
/// Ensures that each dataset contains a SQL query.
/// </description></item>
/// <item><description>
/// Ensures that generated SQL queries are read-only (<c>SELECT</c> or <c>WITH</c> queries).
/// </description></item>
/// </list>
///
/// If validation fails:
///
/// <list type="bullet">
/// <item><description>
/// The work item is marked for retry.
/// </description></item>
/// <item><description>
/// An auxiliary prompt describing the validation failure may be added to help the language model repair the query.
/// </description></item>
/// </list>
///
/// This stage is typically executed before database-specific query validation stages.
/// </remarks>
public sealed class DefaultModelResponseValidator : IValidationPipelineStage
{
	private readonly DefaultModelResponseValidatorSettings _settings;
	private readonly ILogger _logger;

	public DefaultModelResponseValidator(
		DefaultModelResponseValidatorSettings settings,
		ILoggerFactory? loggerFactory = null)
	{
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		var logFactory = loggerFactory ?? NullLoggerFactory.Instance;
		_logger = logFactory.CreateLogger<DefaultModelResponseValidator>();
	}

	public Task ExecuteAsync(ValidationContext context, CancellationToken cancellationToken)
	{
		ModelResponseValidationLogs.ValidationStarted(_logger, context.WorkItemId);

		if (string.IsNullOrEmpty(context.ModelResponse.ChartType))
		{
			ModelResponseValidationLogs.ChartTypeMissing(_logger, context.WorkItemId);

			context.Errors.Add("Chart type is not specified.");
			context.MarkForRetry();

			return Task.CompletedTask;
		}

		if (context.ModelResponse.Datasets == null)
		{
			ModelResponseValidationLogs.DatasetsMissing(_logger, context.WorkItemId);

			context.Errors.Add("Failed to parse model response datasets.");
			context.MarkForRetry();

			return Task.CompletedTask;
		}

		if (!context.ModelResponse.Datasets.Any())
		{
			// expected for "none" chart type
			ModelResponseValidationLogs.DatasetsEmpty(_logger, context.WorkItemId);
		}

		foreach (var dataset in context.ModelResponse.Datasets)
		{
			if (string.IsNullOrWhiteSpace(dataset.SqlQuery))
			{
				ModelResponseValidationLogs.EmptyQueryDetected(_logger, context.WorkItemId);

				context.Errors.Add("Model response contains a dataset with empty SQL queries.");
				context.MarkForRetry();

				continue;
			}

			var query = dataset.SqlQuery.Trim();
			var statements = SplitStatements(query);

			if (!_settings.AllowMultipleStatements)
			{
				if (statements.Count > 1)
				{
					ModelResponseValidationLogs.MultipleStatementsViolation(_logger, context.WorkItemId);

					context.Errors.Add("Multiple SQL statements are not allowed.");
					context.MarkForRetry();

					context.RetryAuxiliaryPrompts.Add(string.Format(SingleStatementAuxiliaryPrompt, query));

					continue;
				}

				query = query.TrimEnd(';');
			}

			dataset.SqlQuery = query;

			foreach (var statement in statements)
			{
				if (!IsSelectQuery(statement))
				{
					ModelResponseValidationLogs.NonSelectQueryDetected(_logger, context.WorkItemId);

					context.Errors.Add("Model response contains a non-select SQL.");
					context.MarkForRetry();

					context.RetryAuxiliaryPrompts.Add(string.Format(OnlySelectAuxiliaryPrompt, statement));
				}
			}
		}

		ModelResponseValidationLogs.ValidationCompleted(_logger, context.WorkItemId, context.Errors.Count);

		return Task.CompletedTask;
	}

	private static bool IsSelectQuery(string query)
	{
		return query.StartsWith("select", StringComparison.OrdinalIgnoreCase)
			|| query.StartsWith("with", StringComparison.OrdinalIgnoreCase);
	}

	private static IReadOnlyList<string> SplitStatements(string query)
	{
		return query.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
	}

	private const string OnlySelectAuxiliaryPrompt = """
		The previously generated SQL query was rejected during validation.

		Only SELECT queries are allowed.
		Do NOT generate INSERT, UPDATE, DELETE, ALTER, CREATE, DROP, or other modifying statements.

		Please rewrite the query as a valid SELECT statement.

		Rejected query:
		{0}
		""";

	private const string SingleStatementAuxiliaryPrompt = """
		The previously generated SQL query was rejected during validation.

		Only a single SQL statement is allowed.

		Please rewrite the query as a single SELECT statement.

		Rejected query:
		{0}
		""";
}
