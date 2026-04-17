using Prompt2Plot;

namespace BlazorDemo.Services;

public class FakeGptExecutor : IPromptExecutor
{
	public Task<ModelResponse> ExecuteAsync(PromptContext promptContext, CancellationToken cancellationToken)
	{
		var result = new ModelResponse
		{
			ChartDescription = "Top 5 authors by lines added",
			ChartType = "bar",
			Datasets =
			[
				new ModelResponseDataset
				{
					Label = "Lines Added",
					SqlQuery = """
					           SELECT
					             author AS label,
					             sum(lines_added) AS value
					           FROM git.commits
					           GROUP BY author
					           ORDER BY value DESC
					           LIMIT 5
					           """
				}
			]
		};

		return Task.FromResult(result);
	}
}
