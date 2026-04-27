using System.ClientModel;
using System.ClientModel.Primitives;
using OpenAI;
using OpenAI.Chat;

namespace Prompt2Plot.OpenAI;

internal static class GptPromptExecutorSettingsExtensions
{
	public static ChatClient GetChatClient(this GptPromptExecutorSettings settings)
	{
		if (settings.ChatClient != null)
		{
			return settings.ChatClient;
		}

		ArgumentNullException.ThrowIfNull(settings.HttpClientFactory, nameof(settings.HttpClientFactory));
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.HttpClientName, nameof(settings.HttpClientName));
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey, nameof(settings.ApiKey));
		ArgumentException.ThrowIfNullOrWhiteSpace(settings.Model, nameof(settings.Model));

		var httpClient = settings.HttpClientFactory.CreateClient(settings.HttpClientName);

		var openaiClient = new OpenAIClient(
			new ApiKeyCredential(settings.ApiKey),
			new OpenAIClientOptions
			{
				Transport = new HttpClientPipelineTransport(httpClient),

			}
		);

		return openaiClient.GetChatClient(settings.Model);
	}
}
