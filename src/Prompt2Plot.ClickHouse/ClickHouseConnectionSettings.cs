namespace Prompt2Plot.ClickHouse;

public sealed class ClickHouseConnectionSettings
{
	public required string ConnectionString { get; init; }
	public required IHttpClientFactory HttpClientFactory { get; set; }
	public required string HttpClientName { get; set; }
}
