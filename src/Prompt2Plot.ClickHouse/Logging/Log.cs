using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse.Logging;

internal static partial class Log
{
	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Warning,
		Message = "Failed to refresh ClickHouse schema.")]
	public static partial void SchemaRefreshFailed(ILogger logger, Exception exception);
}
