namespace Prompt2Plot.Contracts;

/// <summary>
/// Describes a field returned in a query result dataset.
/// </summary>
/// <remarks>
/// Each field corresponds to a column in the SQL query result and provides
/// metadata used by visualization components to interpret the returned data.
/// </remarks>
public sealed class PlotField
{
	/// <summary>
	/// Gets or sets the name of the field.
	/// </summary>
	/// <remarks>
	/// This value corresponds to the column name returned by the SQL query.
	/// </remarks>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the data type of the field.
	/// </summary>
	/// <remarks>
	/// The type is inferred from the underlying database column type and mapped
	/// to a visualization-friendly <see cref="PlotFieldType"/>.
	/// </remarks>
	public PlotFieldType Type { get; set; } = PlotFieldType.Object;
}
