using System.Text;
using System.Text.Json;
using OpenAI.Chat;

namespace Prompt2Plot.OpenAI;

public class GptStructuredPromptExecutor : IPromptExecutor
{
	protected virtual IChartType[] SupportedChartTypes => [];

	private readonly ChatClient _client;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
	};

	public GptStructuredPromptExecutor(ChatClient client)
	{
		ArgumentNullException.ThrowIfNull(client);

		_client = client;
	}

	public async Task<ModelResponse> ExecuteAsync(PromptContext promptContext, CancellationToken cancellationToken)
	{
		var chartTypesEnum = SupportedChartTypes.Length == 0
			? string.Empty
			: $", \"enum\": [{string.Join(", ", SupportedChartTypes.Select(ct => $"\"{ct.Name}\""))}]";

		var jsonSchema = $$"""
			{
			  "type": "object",
			  "properties": {
			    "ChartType": { "type": "string"{{chartTypesEnum}} },
			    "Datasets": {
			      "type": "array",
			      "items": {
			        "type": "object",
			        "properties": {
			          "Label": { "type": "string" },
			          "SqlQuery": { "type": "string" }
			        },
			        "required": ["Label", "SqlQuery"],
			        "additionalProperties": false
			      }
			    },
			    "ChartDescription": { "type": "string" }
			  },
			  "required": ["ChartType", "Datasets", "ChartDescription"],
			  "additionalProperties": false
			}
			""";

		ChatCompletionOptions options = new()
		{
			ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
				jsonSchemaFormatName: "chart_sql_building",
				jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes(jsonSchema)),
				jsonSchemaFormatDescription: "Schema for chart building from SQL queries",
				jsonSchemaIsStrict: true),
		};

		var response = await _client.CompleteChatAsync(
			[
				ChatMessage.CreateSystemMessage(promptContext.Prompt),
				ChatMessage.CreateUserMessage(promptContext.NaturalLanguageRequest),
			],
			options,
			cancellationToken);

		if (response.Value.Content.Count == 0)
		{
			throw new InvalidOperationException("Empty response from model.");
		}

		var json = response.Value.Content[0].Text;

		var modelResponse = JsonSerializer.Deserialize<ModelResponse>(json, JsonOptions);

		if (modelResponse == null)
		{
			throw new InvalidOperationException("Failed to parse model response JSON.");
		}

		return modelResponse;
	}
}
