namespace Prompt2Plot;

[Serializable]
public sealed class DatabaseResponse
{
	public List<PlotField> Fields { get; set; } = [];
	public List<Dictionary<string, object?>> Data { get; set; } = [];
	public string? Error { get; init; }
}
