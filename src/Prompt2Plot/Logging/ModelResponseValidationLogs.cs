using Microsoft.Extensions.Logging;

namespace Prompt2Plot.Logging;

internal static partial class ModelResponseValidationLogs
{
	[LoggerMessage(
		EventId = 30,
		Level = LogLevel.Debug,
		Message = "Model response validation started. WorkItemId: {WorkItemId}.")]
	public static partial void ValidationStarted(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 31,
		Level = LogLevel.Warning,
		Message = "Chart type missing in model response. WorkItemId: {WorkItemId}.")]
	public static partial void ChartTypeMissing(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 32,
		Level = LogLevel.Warning,
		Message = "Datasets missing or empty in model response. WorkItemId: {WorkItemId}.")]
	public static partial void DatasetsMissing(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 33,
		Level = LogLevel.Warning,
		Message = "Dataset contains an empty SQL query. WorkItemId: {WorkItemId}.")]
	public static partial void EmptyQueryDetected(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 34,
		Level = LogLevel.Warning,
		Message = "Non-SELECT SQL query detected during validation. WorkItemId: {WorkItemId}.")]
	public static partial void NonSelectQueryDetected(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 35,
		Level = LogLevel.Warning,
		Message = "Multiple SQL statements detected but are not allowed. WorkItemId: {WorkItemId}.")]
	public static partial void MultipleStatementsViolation(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 36,
		Level = LogLevel.Debug,
		Message = "Model response validation completed. WorkItemId: {WorkItemId}. Errors: {ErrorCount}.")]
	public static partial void ValidationCompleted(ILogger logger, ulong workItemId, int errorCount);
}
