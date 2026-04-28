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

	public uint MaxDatasets { get; set; } = 5;

	public uint DatasetLabelMaxLength { get; set; } = 100;

	public uint SqlQueryMaxLength { get; set; } = 2500;

	public uint ChartDescriptionMaxLength { get; set; } = 500;
}
