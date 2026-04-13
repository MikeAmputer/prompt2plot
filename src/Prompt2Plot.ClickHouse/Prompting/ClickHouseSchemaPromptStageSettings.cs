namespace Prompt2Plot.ClickHouse;

public sealed class ClickHouseSchemaPromptStageSettings
{
	/// <summary>
	/// Gets the connection settings used to create ClickHouse connections.
	/// </summary>
	public required ClickHouseConnectionSettings ConnectionSettings { get; init; }

	public required string[] IncludedDatabases { get; init; }

	public (string database, string table)[] IncludedTables { get; init; } = [];
	public (string database, string table)[] ExcludedTables { get; init; } = [];
	public string[] ExcludedEngines { get; init; } = ["MaterializedView"];
	public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(30);
}
