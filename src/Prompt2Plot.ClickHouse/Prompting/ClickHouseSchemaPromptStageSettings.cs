namespace Prompt2Plot.ClickHouse;

/// <summary>
/// Provides configuration for <see cref="ClickHouseSchemaPromptStage"/>.
/// </summary>
public sealed class ClickHouseSchemaPromptStageSettings
{
	/// <summary>
	/// Gets the connection settings used to create ClickHouse connections.
	/// </summary>
	public required ClickHouseConnectionSettings ConnectionSettings { get; init; }

	/// <summary>
	/// Gets the list of databases whose tables should be included in the schema prompt.
	/// </summary>
	/// <remarks>
	/// At least one database must be specified. Tables outside these databases
	/// are ignored when building the schema prompt.
	/// </remarks>
	public required string[] IncludedDatabases { get; init; }

	/// <summary>
	/// Gets the list of tables that should be explicitly included in the schema prompt.
	/// </summary>
	/// <remarks>
	/// When specified, only the listed tables are included.
	/// Each entry is defined by a <c>(database, table)</c> pair.
	/// If empty, all tables within <see cref="IncludedDatabases"/> will be included.
	/// </remarks>
	public (string database, string table)[] IncludedTables { get; init; } = [];

	/// <summary>
	/// Gets the list of tables that should be excluded from the schema prompt.
	/// </summary>
	/// <remarks>
	/// Each entry is defined by a <c>(database, table)</c> pair.
	/// Excluded tables are ignored even if their database appears in
	/// <see cref="IncludedDatabases"/>.
	/// </remarks>
	public (string database, string table)[] ExcludedTables { get; init; } = [];

	/// <summary>
	/// Gets the list of table engines that should be excluded.
	/// </summary>
	/// <remarks>
	/// By default, <c>MaterializedView</c> tables are excluded.
	/// </remarks>
	public string[] ExcludedEngines { get; init; } = ["MaterializedView"];

	/// <summary>
	/// Gets the duration for which the generated schema prompt is cached.
	/// </summary>
	/// <remarks>
	/// During this period, subsequent prompt executions reuse the cached schema
	/// instead of querying ClickHouse again.
	/// </remarks>
	public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(30);
}
