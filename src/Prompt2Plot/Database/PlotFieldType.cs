namespace Prompt2Plot;

/// <summary>
/// Represents the data type of a field returned in a query result.
/// </summary>
/// <remarks>
/// These types provide a simplified abstraction over database-specific
/// column types and are used to guide visualization components when
/// interpreting query results.
/// </remarks>
public enum PlotFieldType
{
	/// <summary>
	/// A complex or unknown type.
	/// </summary>
	Object,

	/// <summary>
	/// A boolean value.
	/// </summary>
	Boolean,

	/// <summary>
	/// A numeric value.
	/// </summary>
	Number,

	/// <summary>
	/// A string value.
	/// </summary>
	String,

	/// <summary>
	/// A date or timestamp value.
	/// </summary>
	DateTime,
}
