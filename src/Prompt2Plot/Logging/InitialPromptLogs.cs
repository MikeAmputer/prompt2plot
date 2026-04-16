using Microsoft.Extensions.Logging;

namespace Prompt2Plot.Logging;

internal static partial class InitialPromptLogs
{
	[LoggerMessage(
		EventId = 20,
		Level = LogLevel.Debug,
		Message = "Building initial prompt template.")]
	public static partial void PromptBuildStarted(ILogger logger);

	[LoggerMessage(
		EventId = 21,
		Level = LogLevel.Debug,
		Message = "Initial prompt constructed. PromptLength: {PromptLength}.")]
	public static partial void PromptBuildCompleted(ILogger logger, int promptLength);

	[LoggerMessage(
		EventId = 22,
		Level = LogLevel.Debug,
		Message = "Initial prompt appended to prompt context. AddedPromptLength: {AddedPromptLength}. TotalPromptLength: {TotalPromptLength}. WorkItemId: {WorkItemId}.")]
	public static partial void PromptAppended(
		ILogger logger,
		int addedPromptLength,
		int totalPromptLength,
		ulong workItemId);
}
