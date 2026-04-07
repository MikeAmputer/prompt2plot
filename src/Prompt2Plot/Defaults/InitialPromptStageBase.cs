namespace Prompt2Plot.Defaults;

public abstract class InitialPromptStageBase : IPromptPipelineStage
{
	protected abstract string SqlDialect { get; }
	protected abstract IChartType[] SupportedChartTypes { get; }

	private readonly Lazy<string> _prompt;

	protected InitialPromptStageBase()
	{
		_prompt = new Lazy<string>(() =>
			{
				var chartTypeList = string.Join(
					Environment.NewLine,
					SupportedChartTypes.Select(ct => $"- {ct.ToPromptString()}"));

				return string.Format(Template, SqlDialect, chartTypeList);
			},
			isThreadSafe: true);
	}

	public Task ExecuteAsync(PromptContext context, CancellationToken cancellationToken)
	{
		context.Prompt += _prompt.Value;

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

		Always map field names in the SQL to the required chart fields in the SELECT clause.
		Order results logically for readability.
		Use data sampling constraints to keep datasets readable.
		Do not add explanations or extra text.
		Do not expose database structure in labels and description.

		""";
}
