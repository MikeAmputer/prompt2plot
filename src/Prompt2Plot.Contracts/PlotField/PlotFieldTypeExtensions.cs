using Prompt2Plot.Contracts.Constants;

namespace Prompt2Plot.Contracts;

public static class PlotFieldTypeExtensions
{
	public static string ToContractString(this PlotFieldType type) => type switch
	{
		PlotFieldType.Object => PlotFieldTypes.Object,
		PlotFieldType.Boolean => PlotFieldTypes.Boolean,
		PlotFieldType.Number => PlotFieldTypes.Number,
		PlotFieldType.String => PlotFieldTypes.String,
		PlotFieldType.DateTime => PlotFieldTypes.DateTime,
		_ => throw new ArgumentOutOfRangeException(nameof(type))
	};

	public static PlotFieldType Parse(string value) => value switch
	{
		PlotFieldTypes.Object => PlotFieldType.Object,
		PlotFieldTypes.Boolean => PlotFieldType.Boolean,
		PlotFieldTypes.Number => PlotFieldType.Number,
		PlotFieldTypes.String => PlotFieldType.String,
		PlotFieldTypes.DateTime => PlotFieldType.DateTime,
		_ => throw new ArgumentException($"Unknown PlotFieldType '{value}'")
	};

	/// <summary>
	/// Maps a CLR type to a corresponding <see cref="PlotFieldType"/>.
	/// </summary>
	/// <param name="type">The CLR type returned by the database provider.</param>
	/// <param name="additionalTypeMappings">
	/// Additional database-specific mappings supplied by a query executor.
	/// </param>
	/// <returns>The mapped <see cref="PlotFieldType"/>.</returns>
	/// <remarks>
	/// This method provides default mappings for common CLR types such as
	/// numeric primitives, strings, booleans, and date/time values.
	///
	/// Executors may supply additional mappings to support database-specific
	/// types through <paramref name="additionalTypeMappings"/>.
	/// </remarks>
	public static PlotFieldType MapToPlotFieldType(Type type, Dictionary<Type, PlotFieldType> additionalTypeMappings)
	{
		if (Nullable.GetUnderlyingType(type) is { } underlying)
		{
			type = underlying;
		}

		if (additionalTypeMappings.TryGetValue(type, out var plotFieldType))
		{
			return plotFieldType;
		}

		if (type == typeof(string) || type == typeof(char))
		{
			return PlotFieldType.String;
		}

		if (type == typeof(byte) || type == typeof(sbyte) ||
		    type == typeof(short) || type == typeof(ushort) ||
		    type == typeof(int) || type == typeof(uint) ||
		    type == typeof(long) || type == typeof(ulong) ||
		    type == typeof(float) || type == typeof(double) ||
		    type == typeof(decimal))
		{
			return PlotFieldType.Number;
		}

		if (type == typeof(bool))
		{
			return PlotFieldType.Boolean;
		}

		if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
		{
			return PlotFieldType.DateTime;
		}

		return PlotFieldType.Object;
	}
}
