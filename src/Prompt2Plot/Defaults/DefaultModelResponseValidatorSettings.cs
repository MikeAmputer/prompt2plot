namespace Prompt2Plot.Defaults;

/// <summary>
/// Provides configuration for <see cref="DefaultModelResponseValidator"/>.
/// </summary>
public sealed class DefaultModelResponseValidatorSettings
{
	/// <summary>
	/// Gets or sets a value indicating whether SQL queries containing multiple
	/// statements are allowed.
	/// </summary>
	/// <remarks>
	/// When <c>false</c> (default), queries containing multiple statements
	/// separated by semicolons are rejected during validation.
	///
	/// This helps prevent potentially unsafe queries and simplifies
	/// downstream query execution logic.
	/// </remarks>
	public bool AllowMultipleStatements { get; set; } = false;
}
