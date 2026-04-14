using Microsoft.Extensions.Logging;

namespace Prompt2Plot.ClickHouse.Logging;

internal static partial class QueryExecutionLogs
{
	[LoggerMessage(
		EventId = 20,
		Level = LogLevel.Debug,
		Message = "Executing ClickHouse query. QueryId: {QueryId}.")]
	public static partial void QueryExecutionStarted(ILogger logger, string queryId);

	[LoggerMessage(
		EventId = 21,
		Level = LogLevel.Debug,
		Message =
			"ClickHouse query execution completed. QueryId: {QueryId}. RowCount: {RowCount}. ColumnCount: {ColumnCount}.")]
	public static partial void QueryExecutionCompleted(
		ILogger logger,
		string queryId,
		int rowCount,
		int columnCount);

	[LoggerMessage(
		EventId = 22,
		Level = LogLevel.Warning,
		Message = "ClickHouse query execution failed. QueryId: {QueryId}.")]
	public static partial void QueryExecutionFailed(
		ILogger logger,
		string queryId,
		Exception exception);

	[LoggerMessage(
		EventId = 23,
		Level = LogLevel.Debug,
		Message = "Attempting to cancel ClickHouse query. QueryId: {QueryId}.")]
	public static partial void QueryCancellationRequested(
		ILogger logger,
		string queryId);

	[LoggerMessage(
		EventId = 24,
		Level = LogLevel.Warning,
		Message = "Failed to cancel ClickHouse query. QueryId: {QueryId}.")]
	public static partial void QueryCancellationFailed(
		ILogger logger,
		string queryId,
		Exception exception);
}
