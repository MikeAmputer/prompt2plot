using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Prompt2Plot;

/// <summary>
/// A channel-based queue for work items that enforces uniqueness by work item ID and tracks in-progress items.
/// </summary>
internal sealed class DeduplicatingWorkItemChannel
{
	private readonly Channel<WorkItem> _channel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
	{
		SingleReader = false,
		SingleWriter = false,
	});

	private readonly ConcurrentHashSet<ulong> _processing = new();
	private int _count;

	public int Count => Volatile.Read(ref _count);

	public bool TryWrite(WorkItem item)
	{
		ArgumentNullException.ThrowIfNull(item);

		if (!_processing.TryAdd(item.Id))
		{
			return false;
		}

		var written = _channel.Writer.TryWrite(item);

		if (written)
		{
			Interlocked.Increment(ref _count);
		}

		return written;
	}

	public bool TryRead(out WorkItem? item)
	{
		var success = _channel.Reader.TryRead(out item);

		if (success)
		{
			Interlocked.Decrement(ref _count);
		}

		return success;
	}

	public async IAsyncEnumerable<WorkItem> GetConsumingEnumerable(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken))
		{
			Interlocked.Decrement(ref _count);

			yield return item;
		}
	}

	public void Complete()
	{
		_channel.Writer.Complete();
	}

	public bool TryAcknowledge(ulong workItemId)
	{
		return _processing.TryRemove(workItemId);
	}
}
