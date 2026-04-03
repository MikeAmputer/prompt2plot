namespace Prompt2Plot;

[Serializable]
public sealed class PlotField
{
	public string Name { get; set; } = string.Empty;
	public PlotFieldType Type { get; set; } = PlotFieldType.Object;
}
