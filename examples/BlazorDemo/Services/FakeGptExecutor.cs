using Prompt2Plot;

namespace BlazorDemo.Services;

public class FakeGptExecutor : IPromptExecutor
{
	public Task<ModelResponse> ExecuteAsync(PromptContext promptContext, CancellationToken cancellationToken)
	{
		var result = new ModelResponse();

		if (promptContext.NaturalLanguageRequest.Contains("bar", StringComparison.InvariantCultureIgnoreCase))
		{
			result = BarResponse;
		}
		else if (promptContext.NaturalLanguageRequest.Contains("bubble", StringComparison.InvariantCultureIgnoreCase))
		{
			result = BubbleResponse;
		}

		return Task.FromResult(result);
	}

	private static readonly ModelResponse BarResponse = new ModelResponse
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

	private static readonly ModelResponse BubbleResponse = new ModelResponse
	{
		ChartDescription = "Author productivity: files modified vs lines added",
		ChartType = "bubble",
		Datasets =
		[
			new ModelResponseDataset
			{
				Label = "Top Contributors",
				SqlQuery = """
				           SELECT
				             author,
				             sum(files_modified) AS files_modified,
				             sum(lines_added) AS lines_added,
				             count() AS commits
				           FROM git.commits
				           GROUP BY author
				           HAVING count() > 100
				           ORDER BY lines_added DESC
				           LIMIT 15
				           """
			},
			// new ModelResponseDataset
			// {
			// 	Label = "Occasional Contributors",
			// 	SqlQuery = """
			// 	           SELECT
			// 	             author,
			// 	             sum(files_modified) AS x,
			// 	             sum(lines_added) AS y,
			// 	             count() AS size
			// 	           FROM git.commits
			// 	           GROUP BY author
			// 	           HAVING count() <= 100
			// 	           ORDER BY y DESC
			// 	           LIMIT 15
			// 	           """
			// }
		]
	};

	private static readonly ModelResponse LineResponse = new ModelResponse
	{
		ChartDescription = "Repository churn over time",
		ChartType = "line",
		Datasets =
		[
			new ModelResponseDataset
			{
				Label = "Lines Added",
				SqlQuery = """
				           SELECT
				             toStartOfMonth(time) AS month,
				             sum(lines_added) AS value
				           FROM git.commits
				           GROUP BY month
				           ORDER BY month
				           """
			},
			new ModelResponseDataset
			{
				Label = "Lines Deleted",
				SqlQuery = """
				           SELECT
				             toStartOfMonth(time) AS month,
				             sum(lines_deleted) AS value
				           FROM git.commits
				           GROUP BY month
				           ORDER BY month
				           """
			}
		]
	};
}
