namespace Prompt2Plot.Contracts;

/// <summary>
/// Represents the data type of a field returned in a query result.
/// </summary>
/// <remarks>
/// These types provide a simplified abstraction over runtime types and database column types
/// and are used to guide visualization components when interpreting query results.
/// </remarks>
public enum PlotFieldType
{
	Object,
	Boolean,
	Number,
	String,
	DateTime,
}
