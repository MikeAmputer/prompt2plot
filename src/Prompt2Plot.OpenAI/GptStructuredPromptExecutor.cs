using System.ClientModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Prompt2Plot.Contracts;

namespace Prompt2Plot.OpenAI;

public sealed class GptStructuredPromptExecutor : IPromptExecutor
{
	private readonly IChartType[] _supportedChartTypes;
	private readonly ushort _maxRetries;

	private readonly ChatClient _client;
	private readonly ChatCompletionOptions _options;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
	};

	public GptStructuredPromptExecutor(GptPromptExecutorSettings settings, ILoggerFactory? loggerFactory = null)
	{
		ArgumentNullException.ThrowIfNull(settings, nameof(settings));

		_supportedChartTypes = settings.SupportedChartTypes;
		_maxRetries = settings.MaxRetries;

		_client = settings.GetChatClient();
		_options = CreateOptions();
	}

	public async Task<ModelResponse?> ExecuteAsync(
		PromptExecutionContext promptContext,
		CancellationToken cancellationToken)
	{
		string? auxiliaryPrompt = null;
		List<string> errorMessages = [];

		for (var i = 0; i <= _maxRetries; i++)
		{
			var (response, exception) = await TryCompleteChat(
				promptContext, _options, auxiliaryPrompt, cancellationToken);

			if (exception != null)
			{
				promptContext.Errors.Add(exception.Message);

				return null;
			}

			var result = TryParse(response, out auxiliaryPrompt, out errorMessages);

			if (result != null)
			{
				return result;
			}
		}

		promptContext.Errors.AddRange(errorMessages);

		return null;
	}

	private const string InvalidJsonAuxiliaryPrompt =
		"Your previous response was invalid JSON. Please strictly follow the schema. Do not include explanations.";

	private const string EmptyResponseErrorMessage = "Empty response from model.";

	private const string InvalidJsonErrorMessage = "Failed to parse model response JSON.";

	private static ModelResponse? TryParse(
		ClientResult<ChatCompletion>? response,
		out string? auxiliaryPrompt,
		out List<string> errorMessages)
	{
		auxiliaryPrompt = null;
		errorMessages = new List<string>(3);

		if (response == null)
		{
			errorMessages.Add(EmptyResponseErrorMessage);
			return null;
		}

		var content = response.Value.Content;

		if (content.Count == 0 || content.All(c => string.IsNullOrWhiteSpace(c.Text)) )
		{
			errorMessages.Add(EmptyResponseErrorMessage);
			return null;
		}

		var json = content.Count == 1
			? content[0].Text
			: string.Concat(
				content
					.Where(c => !string.IsNullOrWhiteSpace(c.Text))
					.Select(c => c.Text));

		try
		{
			var modelResponse = JsonSerializer.Deserialize<ModelResponse>(json, JsonOptions);

			if (modelResponse != null)
			{
				return modelResponse;
			}

			errorMessages.Add(InvalidJsonErrorMessage);
			errorMessages.Add(json);

			auxiliaryPrompt = InvalidJsonAuxiliaryPrompt;
			return null;

		}
		catch (JsonException exception)
		{
			errorMessages.Add(InvalidJsonErrorMessage);
			errorMessages.Add(exception.Message);
			errorMessages.Add(json);

			auxiliaryPrompt = InvalidJsonAuxiliaryPrompt;
			return null;
		}
	}

	private async Task<(ClientResult<ChatCompletion>? Result, Exception? Exception)> TryCompleteChat(
		PromptExecutionContext promptContext,
		ChatCompletionOptions options,
		string? auxiliaryPrompt,
		CancellationToken cancellationToken)
	{
		var userMessage = auxiliaryPrompt == null
			? promptContext.NaturalLanguageRequest
			: $"{auxiliaryPrompt} Original request: {promptContext.NaturalLanguageRequest}";

		try
		{
			var response = await _client.CompleteChatAsync(
				[
					ChatMessage.CreateSystemMessage(promptContext.Prompt),
					ChatMessage.CreateUserMessage(userMessage),
				],
				options,
				cancellationToken);

			return (response, null);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception exception)
		{
			return (null, exception);
		}
	}

	private ChatCompletionOptions CreateOptions()
	{
		var chartTypesEnum = _supportedChartTypes.Length == 0
			? string.Empty
			: $", \"enum\": [{string.Join(", ", _supportedChartTypes.Select(ct => $"\"{ct.Name}\""))}]";

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

		return new ChatCompletionOptions
		{
			ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
				jsonSchemaFormatName: "chart_sql_building",
				jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes(jsonSchema)),
				jsonSchemaFormatDescription: "Schema for chart building from SQL queries",
				jsonSchemaIsStrict: true),
		};
	}
}
