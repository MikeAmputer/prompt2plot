using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse.Logging;

internal static partial class SchemaPromptLogs
{
	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Debug,
		Message = "Refreshing ClickHouse schema cache.")]
	public static partial void SchemaRefreshStarted(ILogger logger);

	[LoggerMessage(
		EventId = 2,
		Level = LogLevel.Debug,
		Message = "ClickHouse schema cache refreshed. Tables discovered: {TableCount}. PromptLength: {PromptLength}.")]
	public static partial void SchemaRefreshCompleted(ILogger logger, int tableCount, int promptLength);

	[LoggerMessage(
		EventId = 3,
		Level = LogLevel.Warning,
		Message = "Failed to refresh ClickHouse schema.")]
	public static partial void SchemaRefreshFailed(ILogger logger, Exception exception);

	[LoggerMessage(
		EventId = 4,
		Level = LogLevel.Warning,
		Message = "No ClickHouse tables discovered for the configured schema filters.")]
	public static partial void NoTablesDiscovered(ILogger logger);
}
