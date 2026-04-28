using Prompt2Plot;

namespace BlazorDemo.Services;

public class FakeGptExecutor : IPromptExecutor
{
	public Task<ModelResponse?> ExecuteAsync(PromptExecutionContext promptContext, CancellationToken cancellationToken)
	{
		ModelResponse? result = null;

		if (promptContext.NaturalLanguageRequest.Contains("bar", StringComparison.InvariantCultureIgnoreCase))
		{
			result = BarResponse;
		}
		else if (promptContext.NaturalLanguageRequest.Contains("bubble", StringComparison.InvariantCultureIgnoreCase))
		{
			result = BubbleResponse;
		}
		else if (promptContext.NaturalLanguageRequest.Contains("line", StringComparison.InvariantCultureIgnoreCase))
		{
			result = LineResponse;
		}
		else if (promptContext.NaturalLanguageRequest.Contains("pie", StringComparison.InvariantCultureIgnoreCase))
		{
			result = PieResponse;
		}
		else if (promptContext.NaturalLanguageRequest.Contains("table", StringComparison.InvariantCultureIgnoreCase))
		{
			result = TableResponse;
		}
		else if (promptContext.NaturalLanguageRequest.Contains("none", StringComparison.InvariantCultureIgnoreCase))
		{
			result = NoneResponse;
		}

		return Task.FromResult(result);
	}

	private static readonly ModelResponse BarResponse = new ModelResponse
	{
		ChartDescription = "Top 10 authors by lines added",
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
				           LIMIT 10
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
				           OFFSET 1
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

	private static readonly ModelResponse PieResponse = new ModelResponse
	{
		ChartDescription = "Share of lines added by file extension",
		ChartType = "pie",
		Datasets =
		[
			new ModelResponseDataset
			{
				Label = "Lines Added Share",
				SqlQuery = """
				           SELECT
				             file_extension AS label,
				             sum(lines_added) AS value
				           FROM git.file_changes
				           WHERE file_extension != ''
				           GROUP BY file_extension
				           ORDER BY value DESC
				           LIMIT 6
				           """
			}
		]
	};

	private static readonly ModelResponse TableResponse = new ModelResponse
	{
		ChartDescription = "Latest commits with change statistics",
		ChartType = "table",
		Datasets =
		[
			new ModelResponseDataset
			{
				Label = "Recent Commits",
				SqlQuery = """
				           SELECT
				             time,
				             author,
				             substring(hash, 1, 8) AS commit,
				             files_added,
				             files_modified,
				             files_deleted,
				             lines_added,
				             lines_deleted
				           FROM git.commits
				           ORDER BY time DESC
				           LIMIT 200
				           """
			}
		]
	};


	private static readonly ModelResponse NoneResponse = new ModelResponse
	{
		ChartDescription = "",
		ChartType = "none",
		Datasets = []
	};
}
