using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot.ClickHouse;

public static class BuilderExtensions
{
	public static PromptPipelineBuilder AddClickHouseSchemaPromptStage(
		this PromptPipelineBuilder builder,
		Func<IServiceProvider, object?, ClickHouseSchemaPromptStageSettings> settingsProvider)
	{
		return builder.AddStage<ClickHouseSchemaPromptStage>((sp, key) =>
			new ClickHouseSchemaPromptStage(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	public static ValidationPipelineBuilder AddClickHouseQueryValidationStage(
		this ValidationPipelineBuilder builder,
		Func<IServiceProvider, object?, ClickHouseQueryValidationStageSettings> settingsProvider)
	{
		return builder.AddStage<ClickHouseQueryValidationStage>((sp, key) =>
			new ClickHouseQueryValidationStage(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	public static WorkflowServiceBuilder UseClickHouseQueryExecutor(
		this WorkflowServiceBuilder builder,
		Func<IServiceProvider, ClickHouseQueryExecutorSettings> settingsProvider)
	{
		builder.ServiceCollection.AddClickHouseQueryExecutor(builder.WorkflowKey, settingsProvider);

		return builder.WithSqlQueryExecutor<ClickHouseQueryExecutor>();
	}

	public static WorkflowServiceBuilder UseClickHouseQueryExecutor(
		this WorkflowServiceBuilder builder,
		Func<IServiceProvider, ClickHouseConnectionSettings> settingsProvider)
	{
		builder.ServiceCollection.AddClickHouseQueryExecutor(builder.WorkflowKey, settingsProvider);

		return builder.WithSqlQueryExecutor<ClickHouseQueryExecutor>();
	}
}
