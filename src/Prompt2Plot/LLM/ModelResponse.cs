namespace Prompt2Plot;

[Serializable]
public sealed class ModelResponse
{
	public string? ChartType { get; set; }
	public IEnumerable<ModelResponseDataset>? Datasets { get; set; }
	public string? ChartDescription { get; set; }
}

[Serializable]
public sealed class ModelResponseDataset
{
	public string? Label { get; set; }
	public string? SqlQuery { get; set; }
}
