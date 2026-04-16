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

		You must design a chart that can represent data for user request:
		- Choose a chart type using supported chart types list
		- Decide how many datasets the chart will contain
		- Write valid {0} SQL queries for each dataset, to fetch data for the selected chart
		- Apply a short descriptive label for each dataset
		- Add a full chart description for a user, making it easy to understand
		- Return only valid JSON matching this schema:
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

		Supported chart types:
		{1}

		Always alias fields in the SQL to the required chart fields in the SELECT clause.
		Order results logically for readability.
		Use data sampling constraints to keep datasets readable.
		Do not add explanations or extra text.
		Do not expose database structure in labels and description.
		Do not use any comments for queries.

		""";
}
