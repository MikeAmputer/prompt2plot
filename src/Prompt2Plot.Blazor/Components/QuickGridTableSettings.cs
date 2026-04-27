namespace Prompt2Plot.Blazor.Components;

public sealed class QuickGridTableSettings
{
	public bool EnablePagination { get; init; } = true;

	public int PageSize { get; init; } = 10;

	public bool AllowSorting { get; init; } = true;

	public bool HumanizeHeaders { get; init; } = true;

	public string TableClass { get; init; } = "table table-striped table-sm";
}
