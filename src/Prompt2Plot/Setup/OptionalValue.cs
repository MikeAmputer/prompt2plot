namespace Prompt2Plot;

internal readonly struct OptionalValue<TValue>
{
	public bool HasValue { get; }

	private TValue? Value { get; }

	public OptionalValue(TValue? value) : this()
	{
		Value = value;
		HasValue = true;
	}

	public static implicit operator OptionalValue<TValue>(TValue? value) => new(value);

	public static explicit operator TValue?(OptionalValue<TValue> optionalValue) => optionalValue.Value;

	public TValue? OrElseValue(TValue? elseValue)
	{
		return OrElse(() => elseValue);
	}

	public TValue? OrDefault()
	{
		return OrElse(() => default);
	}

	public TValue? OrThrow(string? name = null)
	{
		return OrElse(() => throw new InvalidOperationException(
			$"Required optional value '{name ?? "unknown"}' not set."));
	}

	public TValue NotNullOrThrow(string? name = null)
	{
		return ThrowIfNull(name);
	}

	public void ThrowIfSet(string? name = null)
	{
		if (HasValue)
		{
			throw new InvalidOperationException($"Optional value '{name ?? "unknown"}' already set.");
		}
	}

	private TValue? OrElse(Func<TValue?> elseValueProvider)
	{
		ArgumentNullException.ThrowIfNull(elseValueProvider);

		return HasValue ? Value : elseValueProvider();
	}

	private TValue ThrowIfNull(string? name = null)
	{
		if (!HasValue)
		{
			throw new InvalidOperationException($"Required optional value '{name ?? "unknown"}' not set.");
		}

		if (Value == null)
		{
			throw new InvalidOperationException($"Required optional value '{name ?? "unknown"}' is null.");
		}

		return Value;
	}
}
