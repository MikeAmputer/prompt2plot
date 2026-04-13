namespace Prompt2Plot.ClickHouse;

public sealed class ClickHouseQueryValidationStageSettings
{
	/// <summary>
	/// Gets the connection settings used to create ClickHouse connections.
	/// </summary>
	public required ClickHouseConnectionSettings ConnectionSettings { get; init; }
}
