namespace Prompt2Plot.ClickHouse;

/// <summary>
/// Provides configuration for establishing http connection with ClickHouse server.
/// </summary>
public sealed class ClickHouseConnectionSettings
{
	public required string ConnectionString { get; init; }
	public required IHttpClientFactory HttpClientFactory { get; set; }
	public required string HttpClientName { get; set; }
}
