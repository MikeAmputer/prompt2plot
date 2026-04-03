namespace Prompt2Plot;

public interface IChartType
{
	string Name { get; }
	string ToPromptString();
}
