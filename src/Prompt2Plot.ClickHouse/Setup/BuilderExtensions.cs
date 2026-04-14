using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Prompt2Plot.ClickHouse;

public static class BuilderExtensions
{
	/// <summary>
	/// Adds <see cref="ClickHouseSchemaPromptStage"/> prompt pipeline stage
	/// that injects ClickHouse schema information into the generated prompt.
	/// </summary>
	/// <param name="builder">The prompt pipeline builder.</param>
	/// <param name="settingsProvider">
	/// A factory that provides <see cref="ClickHouseSchemaPromptStageSettings"/>
	/// for the current workflow.
	/// </param>
	/// <returns>The pipeline builder.</returns>
	/// <remarks>
	/// This stage queries ClickHouse system tables to build a textual schema
	/// description that helps the language model generate valid SQL queries.
	/// </remarks>
	public static PromptPipelineBuilder AddClickHouseSchemaPromptStage(
		this PromptPipelineBuilder builder,
		Func<IServiceProvider, object?, ClickHouseSchemaPromptStageSettings> settingsProvider)
	{
		return builder.AddStage<ClickHouseSchemaPromptStage>((sp, key) =>
			new ClickHouseSchemaPromptStage(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	/// <summary>
	/// Adds <see cref="ClickHouseQueryValidationStage"/> validation stage
	/// that validates SQL queries generated for ClickHouse.
	/// </summary>
	/// <param name="builder">The validation pipeline builder.</param>
	/// <param name="settingsProvider">
	/// A factory that provides <see cref="ClickHouseQueryValidationStageSettings"/>
	/// for the current workflow.
	/// </param>
	/// <returns>The validation pipeline builder.</returns>
	/// <remarks>
	/// This stage executes explain plan for the generated SQL queries
	/// to detect syntax or semantic errors.
	/// </remarks>
	public static ValidationPipelineBuilder AddClickHouseQueryValidationStage(
		this ValidationPipelineBuilder builder,
		Func<IServiceProvider, object?, ClickHouseQueryValidationStageSettings> settingsProvider)
	{
		return builder.AddStage<ClickHouseQueryValidationStage>((sp, key) =>
			new ClickHouseQueryValidationStage(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	/// <summary>
	/// Configures the workflow to execute generated ClickHouse queries via <see cref="ClickHouseQueryExecutor"/>.
	/// </summary>
	/// <param name="builder">The workflow service builder.</param>
	/// <param name="settingsProvider">
	/// A factory that provides <see cref="ClickHouseQueryExecutorSettings"/>.
	/// </param>
	/// <returns>The workflow service builder.</returns>
	/// <remarks>
	/// This method registers <see cref="ClickHouseQueryExecutor"/> in the service
	/// collection and configures it as the SQL query executor for the workflow.
	/// </remarks>
	public static WorkflowServiceBuilder UseClickHouseQueryExecutor(
		this WorkflowServiceBuilder builder,
		Func<IServiceProvider, ClickHouseQueryExecutorSettings> settingsProvider)
	{
		builder.ServiceCollection.AddClickHouseQueryExecutor(builder.WorkflowKey, settingsProvider);

		return builder.WithSqlQueryExecutor<ClickHouseQueryExecutor>();
	}

	/// <summary>
	/// Configures the workflow to execute generated ClickHouse queries via <see cref="ClickHouseQueryExecutor"/>.
	/// </summary>
	/// <param name="builder">The workflow service builder.</param>
	/// <param name="settingsProvider">
	/// A factory that provides <see cref="ClickHouseConnectionSettings"/>.
	/// </param>
	/// <returns>The workflow service builder.</returns>
	/// <remarks>
	/// This method registers <see cref="ClickHouseQueryExecutor"/> in the service
	/// collection and configures it as the SQL query executor for the workflow.
	/// </remarks>
	public static WorkflowServiceBuilder UseClickHouseQueryExecutor(
		this WorkflowServiceBuilder builder,
		Func<IServiceProvider, ClickHouseConnectionSettings> settingsProvider)
	{
		builder.ServiceCollection.AddClickHouseQueryExecutor(builder.WorkflowKey, settingsProvider);

		return builder.WithSqlQueryExecutor<ClickHouseQueryExecutor>();
	}
}
