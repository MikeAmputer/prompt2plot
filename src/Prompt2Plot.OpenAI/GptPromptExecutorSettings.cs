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

	public uint MaxDatasets { get; init; } = 5;

	public uint DatasetLabelMaxLength { get; init; } = 100;

	public uint SqlQueryMaxLength { get; init; } = 2500;

	public uint ChartDescriptionMaxLength { get; init; } = 500;

	public float Temperature { get; init; } = 0.1f;

	public float TopP { get; init; } = 0.9f;
}
