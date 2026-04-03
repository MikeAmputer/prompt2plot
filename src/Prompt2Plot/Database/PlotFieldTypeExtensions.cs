namespace Prompt2Plot;

public static class PlotFieldTypeExtensions
{
	public static PlotFieldType MapToPlotFieldType(
		this Type type,
		Dictionary<Type, PlotFieldType> additionalTypeMappings)
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
