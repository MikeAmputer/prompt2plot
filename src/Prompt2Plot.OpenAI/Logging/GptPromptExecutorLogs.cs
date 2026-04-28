using Microsoft.Extensions.Logging;
// ReSharper disable InconsistentNaming

namespace Prompt2Plot.OpenAI.Logging;

internal static partial class GptPromptExecutorLogs
{
	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Debug,
		Message = "Executing GPT structured prompt. WorkItemId: {WorkItemId}.")]
	public static partial void ExecutionStarted(
		ILogger logger,
		ulong workItemId);

	[LoggerMessage(
		EventId = 2,
		Level = LogLevel.Debug,
		Message = "Sending request to OpenAI. Attempt: {Attempt}. WorkItemId: {WorkItemId}.")]
	public static partial void OpenAIRequestStarted(
		ILogger logger,
		int attempt,
		ulong workItemId);

	[LoggerMessage(
		EventId = 3,
		Level = LogLevel.Warning,
		Message = "Failed to parse GPT response JSON. Attempt: {Attempt}. MaxRetries: {MaxRetries}. WorkItemId: {WorkItemId}.")]
	public static partial void JsonParseFailed(
		ILogger logger,
		int attempt,
		int maxRetries,
		ulong workItemId);

	[LoggerMessage(
		EventId = 4,
		Level = LogLevel.Warning,
		Message = "OpenAI request failed. WorkItemId: {WorkItemId}.")]
	public static partial void OpenAIRequestFailed(
		ILogger logger,
		ulong workItemId,
		Exception exception);

	[LoggerMessage(
		EventId = 5,
		Level = LogLevel.Debug,
		Message = "GPT response parsed successfully. WorkItemId: {WorkItemId}.")]
	public static partial void ResponseParsed(
		ILogger logger,
		ulong workItemId);

	[LoggerMessage(
		EventId = 6,
		Level = LogLevel.Warning,
		Message = "GPT structured prompt execution failed after retries. WorkItemId: {WorkItemId}. RetryCount: {RetryCount}.")]
	public static partial void ExecutionFailed(
		ILogger logger,
		ulong workItemId,
		int retryCount);
}
