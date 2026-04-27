using OpenAI.Chat;
using Prompt2Plot.Contracts;

namespace Prompt2Plot.OpenAI;

public sealed class GptPromptExecutorSettings
{
	public ChatClient? ChatClient { get; init; }

	public IHttpClientFactory? HttpClientFactory { get; init; }

	public string? HttpClientName { get; init; }

	public string? ApiKey { get; init; }

	public string? Model { get; init; }

	public required IChartType[] SupportedChartTypes { get; init; }

	public required ushort MaxRetries { get; init; }
}
