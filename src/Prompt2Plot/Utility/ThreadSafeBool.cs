namespace Prompt2Plot;

internal sealed class ThreadSafeBool
{
	private int _value = 0;

	public bool Value
	{
		get => Volatile.Read(ref _value) != 0;
		set => Interlocked.Exchange(ref _value, value ? 1 : 0);
	}

	public bool TrySet(bool newValue)
	{
		var newInt = newValue ? 1 : 0;
		var oppositeInt = newValue ? 0 : 1;

		return Interlocked.CompareExchange(ref _value, newInt, oppositeInt) == oppositeInt;
	}
}
