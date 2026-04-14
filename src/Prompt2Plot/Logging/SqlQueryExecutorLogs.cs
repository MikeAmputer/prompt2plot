using Microsoft.Extensions.Logging;

namespace Prompt2Plot.Logging;

internal static partial class SqlQueryExecutorLogs
{
	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Warning,
		Message = "SQL query execution failed.")]
	public static partial void QueryExecutionFailed(ILogger logger, Exception exception);
}
