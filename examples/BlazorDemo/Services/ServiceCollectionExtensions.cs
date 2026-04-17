using System.Net;
using Prompt2Plot;
using Prompt2Plot.ClickHouse;

namespace BlazorDemo.Services;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationServices(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddRazorComponents()
			.AddInteractiveServerComponents();

		services.AddClickHouse();

		services.AddKeyedSingleton<FakeGptExecutor>("blazor-flow");

		services.AddPrompt2Plot(b => b
			.UseInMemoryWorkItemRepository()
			.AddWorkflow("blazor-flow", workflow => workflow
				.WithPromptExecutor<FakeGptExecutor>()
				.UseClickHouseQueryExecutor(ClickHouseSettingsProvider)
				.WithPromptPipeline(prompt => prompt
					.AddInitialPromptStage(sqlDialect: "ClickHouse")
					.AddClickHouseSchemaPromptStage(ClickHouseSchemaPromptStageSettingsProvider))
				.WithValidationPipeline(validation => validation
					.AddModelResponseValidator()
					.AddClickHouseQueryValidationStage(ClickHouseQueryValidationStageSettingsProvider)
					.WithMaxRetries(2)))
		);

		services.AddHostedService<WorkflowHostedService>();

		return services;
	}

	private static IServiceCollection AddClickHouse(this IServiceCollection services)
	{
		services.AddHttpClient("ClickHouse")
			.ConfigureHttpClient((_, httpClient) =>
			{
				httpClient.Timeout = TimeSpan.FromSeconds(60);
			})
			.SetHandlerLifetime(Timeout.InfiniteTimeSpan)
			.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				MaxConnectionsPerServer = 8,
			});

		return services;
	}

	private static ClickHouseConnectionSettings ClickHouseSettingsProvider(IServiceProvider sp) =>
		new()
		{
			ConnectionString =
				"host=prompt2plot-blazor-clickhouse;port=8123;database=git;username=example;password=example;Timeout=60;",
			HttpClientFactory = sp.GetRequiredService<IHttpClientFactory>(),
			HttpClientName = "ClickHouse",
		};

	private static ClickHouseSchemaPromptStageSettings ClickHouseSchemaPromptStageSettingsProvider(
		IServiceProvider sp, object? key) =>
		new()
		{
			ConnectionSettings = ClickHouseSettingsProvider(sp),
			IncludedDatabases = ["git"],
		};

	private static ClickHouseQueryValidationStageSettings ClickHouseQueryValidationStageSettingsProvider(
		IServiceProvider sp, object? key) =>
		new()
		{
			ConnectionSettings = ClickHouseSettingsProvider(sp),
		};
}
