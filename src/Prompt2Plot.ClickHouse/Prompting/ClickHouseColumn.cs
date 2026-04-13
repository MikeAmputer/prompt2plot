using System.Data;
using System.Text;

namespace Prompt2Plot.ClickHouse;

internal class ClickHouseColumn
{
	public string Database { get; init; } = string.Empty;
	public string Table { get; init; } = string.Empty;
	public string Name { get; init; } = string.Empty;
	public string Type { get; init; } = string.Empty;
	public string Comment { get; init; } = string.Empty;

	public static ClickHouseColumn FromReader(IDataReader reader)
	{
		return new ClickHouseColumn
		{
			Database = reader.GetString(0),
			Table = reader.GetString(1),
			Name = reader.GetString(2),
			Type = reader.GetString(3),
			Comment = reader.GetString(4),
		};
	}

	public string ToPromptString()
	{
		var sb = new StringBuilder();
		sb.Append($"    - {Name}: {Type}");
		if (!string.IsNullOrWhiteSpace(Comment))
		{
			sb.Append($"  // {Comment}");
		}

		sb.AppendLine();

		return sb.ToString();
	}
}
