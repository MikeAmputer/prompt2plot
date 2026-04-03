using System.Collections.Concurrent;

namespace Prompt2Plot;

internal sealed class ConcurrentHashSet<T>
	where T : notnull
{
	private readonly ConcurrentDictionary<T, byte> _dict = new();

	public bool TryAdd(T item) => _dict.TryAdd(item, 0);
	public bool TryRemove(T item) => _dict.TryRemove(item, out _);
	public bool ContainsKey(T item) => _dict.ContainsKey(item);
}
