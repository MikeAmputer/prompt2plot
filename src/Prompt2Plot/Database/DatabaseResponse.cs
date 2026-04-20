namespace Prompt2Plot;

/// <summary>
/// Represents the result of executing a SQL query.
/// </summary>
/// <remarks>
/// This object contains the dataset returned by a database executor and is
/// structured in a format suitable for visualization pipelines.
///
/// The response contains:
///
/// <list type="bullet">
/// <item><description>
/// <see cref="Fields"/> — metadata describing the columns returned by the query.
/// The order of fields defines the column index used in <see cref="Rows"/>.
/// </description></item>
/// <item><description>
/// <see cref="Rows"/> — row data represented as positional arrays.
/// Each array element corresponds to the field at the same index in <see cref="Fields"/>.
/// </description></item>
/// <item><description>
/// <see cref="Error"/> — an optional error message if query execution failed.
/// </description></item>
/// </list>
///
/// When <see cref="Error"/> is not <c>null</c>, the query execution failed and
/// <see cref="Fields"/> and <see cref="Rows"/> may be empty.
/// </remarks>
public sealed class DatabaseResponse
{
	/// <summary>
	/// Gets or sets the fields describing the returned dataset columns.
	/// </summary>
	public List<PlotField> Fields { get; set; } = [];

	/// <summary>
	/// Gets or sets the returned rows.
	/// </summary>
	/// <remarks>
	/// Each row is represented as an array of values where the index corresponds
	/// to the field at the same index in <see cref="Fields"/>.
	/// </remarks>
	public List<object?[]> Rows { get; set; } = [];

	/// <summary>
	/// Gets the error message produced during query execution, if any.
	/// </summary>
	/// <remarks>
	/// If this property is not <c>null</c>, the query execution failed and the
	/// returned dataset should be considered invalid.
	/// </remarks>
	public string? Error { get; init; }
}
