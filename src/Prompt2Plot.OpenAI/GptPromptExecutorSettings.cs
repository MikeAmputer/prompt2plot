using OpenAI.Chat;
using Prompt2Plot.Contracts;

namespace Prompt2Plot.OpenAI;

/// <summary>
/// Configuration settings for <see cref="GptStructuredPromptExecutor"/>.
/// </summary>
public sealed class GptPromptExecutorSettings
{
	/// <summary>
	/// Preconfigured <see cref="ChatClient"/> instance used to execute chat completions.
	/// If not provided, a client will be created from the configured API settings.
	/// </summary>
	public ChatClient? ChatClient { get; init; }

	/// <summary>
	/// HTTP client factory used when creating the internal <see cref="ChatClient"/>.
	/// </summary>
	public IHttpClientFactory? HttpClientFactory { get; init; }

	/// <summary>
	/// HTTP client name used when creating the internal <see cref="ChatClient"/>.
	/// </summary>
	public string? HttpClientName { get; init; }

	/// <summary>
	/// API key used when creating the internal <see cref="ChatClient"/>.
	/// </summary>
	public string? ApiKey { get; init; }

	/// <summary>
	/// Language model identifier used when creating the internal <see cref="ChatClient"/>.
	/// </summary>
	public string? Model { get; init; }

	/// <summary>
	/// Supported chart types that the model may select when generating responses.
	/// </summary>
	public required IChartType[] SupportedChartTypes { get; init; }

	/// <summary>
	/// Maximum number of retries if the model response fails JSON validation.
	/// </summary>
	public required ushort MaxRetries { get; init; }

	/// <summary>
	/// Maximum number of datasets the model may generate.
	/// </summary>
	public uint MaxDatasets { get; init; } = 5;

	/// <summary>
	/// Maximum allowed length for dataset labels.
	/// </summary>
	public uint DatasetLabelMaxLength { get; init; } = 100;

	/// <summary>
	/// Maximum allowed length for generated SQL queries.
	/// </summary>
	public uint SqlQueryMaxLength { get; init; } = 2500;

	/// <summary>
	/// Maximum allowed length for chart descriptions.
	/// </summary>
	public uint ChartDescriptionMaxLength { get; init; } = 500;

	/// <summary>
	/// Sampling temperature used by the language model.
	/// Lower values produce more deterministic responses.
	/// </summary>
	public float Temperature { get; init; } = 0.1f;

	/// <summary>
	/// Nucleus sampling parameter controlling token probability mass.
	/// </summary>
	public float TopP { get; init; } = 0.9f;
}
