using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Prompt2Plot.ClickHouse;

internal partial class ClickHouseTable
{
	public string Database { get; init; } = string.Empty;
	public string Name { get; init; } = string.Empty;
	public string Engine { get; init; } = string.Empty;
	public string EngineFull { get; init; } = string.Empty;
	public string SortingKey { get; init; } = string.Empty;
	public ulong TotalRows { get; init; }
	public string Comment { get; init; } = string.Empty;

	public List<ClickHouseColumn> Columns { get; } = [];

	public static ClickHouseTable FromReader(IDataReader reader)
	{
		return new ClickHouseTable
		{
			Database = reader.GetString(0),
			Name = reader.GetString(1),
			Engine = reader.GetString(2),
			EngineFull = reader.GetString(3),
			SortingKey = reader.GetString(4),
			TotalRows = reader.IsDBNull(5) ? 0UL : Convert.ToUInt64(reader.GetValue(5)),
			Comment = reader.GetString(6),
		};
	}

	public string ToPromptString()
	{
		var sb = new StringBuilder();

		sb.AppendLine($"Table {Database}.{Name}");

		if (string.IsNullOrWhiteSpace(EngineFull) ||
		    string.Equals(Engine, DistributedEngine, StringComparison.OrdinalIgnoreCase))
		{
			sb.AppendLine($"  Engine: {Engine}");
		}
		else
		{
			sb.AppendLine($"  Engine: {NormalizeEngine(EngineFull)}");
		}

		if (!string.IsNullOrWhiteSpace(SortingKey))
		{
			sb.AppendLine($"  Sorting key: {SortingKey}");
		}

		sb.AppendLine($"  Total rows: {TotalRows}");

		if (!string.IsNullOrWhiteSpace(Comment))
		{
			sb.AppendLine($"  Comment: {Comment}");
		}

		sb.AppendLine("  Columns:");

		foreach (var col in Columns)
		{
			sb.Append(col.ToPromptString());
		}

		return sb.ToString();
	}

	private const string DistributedEngine = "Distributed";

	[GeneratedRegex(@"\bSETTINGS\b.*$", RegexOptions.Compiled)]
	private static partial Regex SettingsRegexCompiled();

	private static readonly Regex SettingsRegex = SettingsRegexCompiled();

	[GeneratedRegex(@"^Replicated([A-Za-z]*MergeTree)\s*\([^)]*\)", RegexOptions.Compiled)]
	private static partial Regex ReplicatedRegexCompiled();

	private static readonly Regex ReplicatedRegex = ReplicatedRegexCompiled();

	private static string NormalizeEngine(string engineDefinition)
	{
		if (string.IsNullOrEmpty(engineDefinition))
		{
			return engineDefinition;
		}

		var result = engineDefinition;

		result = SettingsRegex.Replace(result, "").TrimEnd();

		result = ReplicatedRegex.Replace(result, "$1");

		return result;
	}
}
