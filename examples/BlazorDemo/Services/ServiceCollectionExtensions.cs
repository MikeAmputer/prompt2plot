using System.Net;
using ApexCharts;
using Polly;
using Prompt2Plot;
using Prompt2Plot.Blazor;
using Prompt2Plot.Blazor.ApexCharts;
using Prompt2Plot.ClickHouse;
using Prompt2Plot.Contracts.Constants;
using Prompt2Plot.OpenAI;

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

		services.AddSingleton<PromptExecutorController>();

		services.AddOpenAi();

		services.AddKeyedSingleton<FakeGptExecutor>(PromptExecutorController.FakeGptFlowKey);

		// Add...  → registers services in DI
		// Use...  → registers infrastructure and configures the builder
		// With... → configures the builder only

		services.AddPrompt2Plot(b => b
			.UseInMemoryWorkItemRepository()
			.AddWorkflow(PromptExecutorController.FakeGptFlowKey, workflow => workflow
				.WithPromptExecutor<FakeGptExecutor>()
				.UseClickHouseQueryExecutor(ClickHouseSettingsProvider)
				.WithPromptPipeline(prompt => prompt
					.AddInitialPromptStage(sqlDialect: "ClickHouse")
					.AddClickHouseSchemaPromptStage(ClickHouseSchemaPromptStageSettingsProvider))
				.WithValidationPipeline(validation => validation
					.AddModelResponseValidator()
					.AddClickHouseQueryValidationStage(ClickHouseQueryValidationStageSettingsProvider)
					.WithMaxRetries(2)))
			.AddWorkflow(PromptExecutorController.ChatGptFlowKey, workflow => workflow
				.UseGptStructuredPromptExecutor(ChatGptPromptExecutorSettingsProvider, ServiceLifetime.Transient)
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

		services.AddApexCharts(e =>
		{
			e.GlobalOptions = new ApexChartBaseOptions
			{
				Debug = true,
				Theme = new Theme { Palette = PaletteType.Palette6 }
			};
		});

		services.AddPlotRendering(setup => setup
			.WithQuickGridTableComponent()
			.WithApexBarChartComponent()
			.WithApexBubbleChartComponent()
			.WithApexLineChartComponent()
			.WithApexPieChartComponent()
			.WithDefaultNoneChartComponent());

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

	private static IServiceCollection AddOpenAi(this IServiceCollection services)
	{
		services.AddHttpClient("OpenAI")
			.ConfigureHttpClient((_, httpClient) =>
			{
				httpClient.Timeout = TimeSpan.FromSeconds(60);
			})
			.SetHandlerLifetime(Timeout.InfiniteTimeSpan)
			.AddStandardResilienceHandler(options =>
			{
				options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

				options.Retry.MaxRetryAttempts = 1;
				options.Retry.Delay = TimeSpan.FromSeconds(1);
				options.Retry.BackoffType = DelayBackoffType.Exponential;
			});

		return services;
	}

	private static ClickHouseConnectionSettings ClickHouseSettingsProvider(IServiceProvider sp)
	{
		var inContainer = Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") == "true";

		return new ClickHouseConnectionSettings
		{
			ConnectionString = inContainer
				? "host=prompt2plot-blazor-clickhouse;port=8123;database=git;username=example;password=example;Timeout=60;"
				: "host=localhost;port=28123;database=git;username=example;password=example;Timeout=60;",
			HttpClientFactory = sp.GetRequiredService<IHttpClientFactory>(),
			HttpClientName = "ClickHouse",
		};
	}

	private static GptPromptExecutorSettings ChatGptPromptExecutorSettingsProvider(IServiceProvider sp) =>
		new()
		{
			SupportedChartTypes = ChartTypes.All,
			MaxRetries = 1,
			HttpClientName = "OpenAI",
			HttpClientFactory = sp.GetRequiredService<IHttpClientFactory>(),
			ApiKey = sp.GetRequiredService<PromptExecutorController>().ApiKey,
			Model = sp.GetRequiredService<PromptExecutorController>().Model,
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
