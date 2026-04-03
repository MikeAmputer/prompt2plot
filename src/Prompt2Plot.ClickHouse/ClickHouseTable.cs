using System.Data;
using System.Text;

namespace Prompt2Plot.ClickHouse;

internal class ClickHouseTable
{
	public string Database { get; init; } = string.Empty;
	public string Name { get; init; } = string.Empty;
	public string Engine { get; init; } = string.Empty;
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
			SortingKey = reader.GetString(3),
			TotalRows = reader.IsDBNull(4) ? 0UL : Convert.ToUInt64(reader.GetValue(4)),
			Comment = reader.GetString(5)
		};
	}

	public string ToPromptString()
	{
		var sb = new StringBuilder();

		sb.AppendLine($"Table {Database}.{Name}");
		sb.AppendLine($"  Engine: {Engine}");

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
}
