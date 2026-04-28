using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prompt2Plot.Logging;

namespace Prompt2Plot.Defaults;

/// <summary>
/// A prompt pipeline stage that generates the base system prompt used for
/// SQL generation and chart design.
/// </summary>
/// <remarks>
/// This stage defines the fundamental instructions for the language model.
/// The prompt describes:
///
/// <list type="bullet">
/// <item><description>The role of the model (SQL + chart generator)</description></item>
/// <item><description>The JSON response schema</description></item>
/// <item><description>The supported visualization types</description></item>
/// <item><description>General query generation guidelines</description></item>
/// </list>
///
/// The prompt is generated once and cached using <see cref="Lazy{T}"/> since
/// it depends only on static configuration provided by
/// <see cref="DefaultInitialPromptStageSettings"/>.
///
/// The generated text is appended to the <see cref="PromptContext.Prompt"/>
/// before other stages extend the prompt with additional information
/// such as database schema or user requests.
/// </remarks>
public sealed class DefaultInitialPromptStage : IPromptPipelineStage
{
	private readonly Lazy<string> _prompt;

	private readonly ILogger _logger;

	public DefaultInitialPromptStage(DefaultInitialPromptStageSettings settings, ILoggerFactory? loggerFactory = null)
	{
		var sqlDialect = settings.SqlDialect;
		var supportedChartTypes = settings.SupportedChartTypes;

		var logFactory = loggerFactory ?? NullLoggerFactory.Instance;
		_logger = logFactory.CreateLogger<DefaultInitialPromptStage>();

		_prompt = new Lazy<string>(() =>
			{
				InitialPromptLogs.PromptBuildStarted(_logger);

				var chartTypeList = string.Join(
					Environment.NewLine,
					supportedChartTypes.Select(ct => $"- {ct.ToPromptString()}"));

				var prompt = string.Format(Template, sqlDialect, chartTypeList);

				InitialPromptLogs.PromptBuildCompleted(_logger, prompt.Length);

				return prompt;
			},
			isThreadSafe: true);
	}

	public Task ExecuteAsync(PromptContext context, CancellationToken cancellationToken)
	{
		context.Prompt += _prompt.Value;

		InitialPromptLogs.PromptAppended(
			_logger,
			_prompt.Value.Length,
			context.Prompt.Length,
			context.WorkItemId);

		return Task.CompletedTask;
	}

	private const string Template = """
		You are an AI SQL and dashboard generator.

		You receive:
		1. A database schema
		2. A natural language request

		Rules:
		Use only tables and columns that are explicitly provided in the schema.
		If the request asks for schema or data modification, ignore that part of the request.
		Return only valid JSON matching this schema:
		{{
		  "ChartType": "string",
		  "Datasets": [
		    {{
		      "Label": "string",
		      "SqlQuery": "string"
		    }}
		  ],
		  "ChartDescription": "string"
		}}

		Chart generation steps:
		- Choose a chart type from the supported chart types list
		- Decide how many datasets the chart contains
		- Write one valid {0} SQL query per dataset
		- Each dataset must have a short descriptive label
		- Provide a clear chart description for the user

		Supported chart types:
		{1}

		SQL query rules:
		Never generate statements that modify database structure or data.
		Return only the fields required by the chart type; do not include extra columns.
		Always preserve the field order and data types defined by the chart type.
		Field names are optional; use descriptive aliases with AS when helpful.
		When returning multiple datasets, ensure they have the same number of fields, with identical field order and data types.
		Exclude rows with NULL values in chart fields using WHERE ... IS NOT NULL.
		When aggregating, group only by chart dimension fields (e.g., category or time bucket).
		Do not group by identifiers, hashes, or raw timestamps.
		Do not join tables unless the relationship is clearly defined by matching columns.
		Ensure each selected field returns a consistent data type across all rows (cast if necessary).
		Order the result set by the column that best represents query relevance so that LIMIT returns meaningful rows.
		Apply data sampling constraints to keep datasets readable.
		Always apply ORDER BY before LIMIT.
		Never invent tables or columns that are not present in the schema.
		Do not include comments in SQL queries.

		Output rules:
		Do not add explanations or extra text.
		Do not expose database structure in labels or chart descriptions.
		Verify that the SQL queries follow all rules and match the chart field specification.
		If the request cannot be answered using only the provided schema, return an empty "Datasets" array.

		""";
}
