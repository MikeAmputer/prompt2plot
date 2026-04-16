using Microsoft.Extensions.Logging;

namespace Prompt2Plot.Logging;

internal static partial class InMemoryRepositoryLogs
{
	[LoggerMessage(
		EventId = 100,
		Level = LogLevel.Information,
		Message = "In-memory work item repository initialized. MaxPending: {MaxPending}. MaxResults: {MaxResults}. MaxWaiters: {MaxWaiters}. ResultChannelCapacity: {ResultChannelCapacity}. PublisherEnabled: {PublisherEnabled}.")]
	public static partial void RepositoryInitialized(
		ILogger logger,
		uint maxPending,
		uint maxResults,
		uint maxWaiters,
		uint resultChannelCapacity,
		bool publisherEnabled);

	[LoggerMessage(
		EventId = 101,
		Level = LogLevel.Debug,
		Message = "Work item added to pending. WorkItemId: {WorkItemId}.")]
	public static partial void WorkItemAdded(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 102,
		Level = LogLevel.Debug,
		Message = "Work item published via external publisher. WorkItemId: {WorkItemId}.")]
	public static partial void WorkItemPublished(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 103,
		Level = LogLevel.Debug,
		Message = "Work item result stored. WorkItemId: {WorkItemId}.")]
	public static partial void ResultStored(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 104,
		Level = LogLevel.Debug,
		Message = "Waiter registered for work item result. WorkItemId: {WorkItemId}.")]
	public static partial void WaiterRegistered(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 105,
		Level = LogLevel.Warning,
		Message = "Pending work item evicted due to repository limits. WorkItemId: {WorkItemId}.")]
	public static partial void PendingEvicted(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 106,
		Level = LogLevel.Warning,
		Message = "Stored result evicted due to repository limits. WorkItemId: {WorkItemId}.")]
	public static partial void ResultEvicted(ILogger logger, ulong workItemId);

	[LoggerMessage(
		EventId = 107,
		Level = LogLevel.Warning,
		Message = "Result waiter evicted due to repository limits. WorkItemId: {WorkItemId}.")]
	public static partial void WaiterEvicted(ILogger logger, ulong workItemId);
}
