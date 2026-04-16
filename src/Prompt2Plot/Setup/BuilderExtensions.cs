using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prompt2Plot.Defaults;
using Prompt2Plot.InMemory;

namespace Prompt2Plot;

public static class BuilderExtensions
{
	public static Prompt2PlotBuilder UseInMemoryWorkItemRepository(
		this Prompt2PlotBuilder builder,
		Func<IServiceProvider, InMemoryWorkItemRepositorySettings> settingsProvider)
	{
		builder.ServiceCollection.AddInMemoryWorkItemRepository(settingsProvider);

		return builder.WithWorkItemRepository<InMemoryWorkItemRepository>();
	}

	public static Prompt2PlotBuilder UseInMemoryWorkItemRepository(this Prompt2PlotBuilder builder)
	{
		builder.ServiceCollection.AddInMemoryWorkItemRepository();

		return builder.WithWorkItemRepository<InMemoryWorkItemRepository>();
	}

	/// <summary>
	/// Adds the <see cref="DefaultInitialPromptStage"/> to the prompt pipeline.
	/// </summary>
	/// <remarks>
	/// This stage generates the base system prompt that instructs the language model
	/// how to convert a natural language request into SQL queries and a visualization
	/// specification.
	///
	/// The prompt includes:
	/// <list type="bullet">
	/// <item><description>Instructions describing the model's role</description></item>
	/// <item><description>The expected JSON response schema</description></item>
	/// <item><description>The supported chart types and their dataset structures</description></item>
	/// <item><description>General query generation guidelines</description></item>
	/// </list>
	///
	/// The provided <paramref name="settingsProvider"/> allows resolving stage
	/// configuration dynamically using the dependency injection container and
	/// the workflow key.
	///
	/// This overload is typically used when configuration depends on runtime
	/// services such as database providers, tenant configuration, or environment
	/// settings.
	/// </remarks>
	/// <param name="builder">
	/// The <see cref="PromptPipelineBuilder"/> used to construct the prompt pipeline.
	/// </param>
	/// <param name="settingsProvider">
	/// A delegate that resolves <see cref="DefaultInitialPromptStageSettings"/>
	/// using the service provider and workflow key.
	/// </param>
	/// <returns>
	/// The <see cref="PromptPipelineBuilder"/> so that additional stages can be chained.
	/// </returns>
	public static PromptPipelineBuilder AddInitialPromptStage(
		this PromptPipelineBuilder builder,
		Func<IServiceProvider, object?, DefaultInitialPromptStageSettings> settingsProvider)
	{
		return builder.AddStage<DefaultInitialPromptStage>((sp, key) =>
			new DefaultInitialPromptStage(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	/// <summary>
	/// Adds the <see cref="DefaultInitialPromptStage"/> to the prompt pipeline
	/// using the provided settings instance.
	/// </summary>
	/// <remarks>
	/// This stage generates the base system prompt that instructs the language model
	/// how to convert a natural language request into SQL queries and a visualization
	/// specification.
	///
	/// The prompt includes:
	/// <list type="bullet">
	/// <item><description>Instructions describing the model's role</description></item>
	/// <item><description>The expected JSON response schema</description></item>
	/// <item><description>The supported chart types and their dataset structures</description></item>
	/// <item><description>General query generation guidelines</description></item>
	/// </list>
	///
	/// This overload is intended for scenarios where the prompt configuration
	/// is static and known during application startup.
	/// settings.
	/// </remarks>
	/// <param name="builder">
	/// The <see cref="PromptPipelineBuilder"/> used to construct the prompt pipeline.
	/// </param>
	/// <param name="settings">
	/// The configuration used to build the initial prompt.
	/// </param>
	/// <returns>
	/// The <see cref="PromptPipelineBuilder"/> so that additional stages can be chained.
	/// </returns>
	public static PromptPipelineBuilder AddInitialPromptStage(
		this PromptPipelineBuilder builder,
		DefaultInitialPromptStageSettings settings)
	{
		return builder.AddStage<DefaultInitialPromptStage>((sp, _) =>
			new DefaultInitialPromptStage(
				settings,
				sp.GetService<ILoggerFactory>()));
	}

	/// <summary>
	/// Adds the <see cref="DefaultInitialPromptStage"/> to the prompt pipeline
	/// using a simplified configuration.
	/// </summary>
	/// <remarks>
	/// This stage generates the base system prompt that instructs the language model
	/// how to convert a natural language request into SQL queries and a visualization
	/// specification.
	///
	/// The prompt includes:
	/// <list type="bullet">
	/// <item><description>Instructions describing the model's role</description></item>
	/// <item><description>The expected JSON response schema</description></item>
	/// <item><description>The supported chart types and their dataset structures</description></item>
	/// <item><description>General query generation guidelines</description></item>
	/// </list>
	///
	/// This overload provides a convenient way to configure the initial prompt
	/// when only the SQL dialect and optional chart types need to be specified.
	///
	/// Internally this method creates a <see cref="DefaultInitialPromptStageSettings"/>
	/// instance using the provided parameters.
	///
	/// If <paramref name="supportedChartTypes"/> is not specified, the default
	/// chart types defined by <see cref="DefaultInitialPromptStageSettings"/> are used.
	/// </remarks>
	/// <param name="builder">
	/// The <see cref="PromptPipelineBuilder"/> used to construct the prompt pipeline.
	/// </param>
	/// <param name="sqlDialect">
	/// The SQL dialect used by the target database (for example: <c>ClickHouse</c>,
	/// <c>PostgreSQL</c>, or <c>MySQL</c>).
	/// </param>
	/// <param name="supportedChartTypes">
	/// Optional set of chart types supported by the workflow.
	/// If not provided, default chart types will be used.
	/// </param>
	/// <returns>
	/// The <see cref="PromptPipelineBuilder"/> so that additional stages can be chained.
	/// </returns>
	public static PromptPipelineBuilder AddInitialPromptStage(
		this PromptPipelineBuilder builder,
		string sqlDialect,
		IChartType[]? supportedChartTypes = null)
	{
		if (supportedChartTypes == null)
		{
			return builder.AddInitialPromptStage(
				new DefaultInitialPromptStageSettings { SqlDialect = sqlDialect });
		}

		return builder.AddInitialPromptStage(
			new DefaultInitialPromptStageSettings
			{
				SqlDialect = sqlDialect,
				SupportedChartTypes = supportedChartTypes,
			});
	}

	/// <summary>
	/// Adds the <see cref="DefaultModelResponseValidator"/> to the validation pipeline.
	/// </summary>
	/// <param name="builder">
	/// The <see cref="ValidationPipelineBuilder"/> used to construct the validation pipeline.
	/// </param>
	/// <param name="settingsProvider">
	/// A delegate that resolves <see cref="DefaultModelResponseValidatorSettings"/>
	/// using the service provider and workflow key.
	/// </param>
	/// <returns>
	/// The <see cref="ValidationPipelineBuilder"/> so that additional stages can be chained.
	/// </returns>
	public static ValidationPipelineBuilder AddModelResponseValidator(
		this ValidationPipelineBuilder builder,
		Func<IServiceProvider, object?, DefaultModelResponseValidatorSettings> settingsProvider)
	{
		return builder.AddStage<DefaultModelResponseValidator>((sp, key) =>
			new DefaultModelResponseValidator(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	/// <summary>
	/// Adds the <see cref="DefaultModelResponseValidator"/> to the validation pipeline.
	/// </summary>
	/// <remarks>
	/// This overload is intended for scenarios where validator configuration
	/// is static and known during application startup.
	/// </remarks>
	/// <param name="builder">
	/// The <see cref="ValidationPipelineBuilder"/> used to construct the validation pipeline.
	/// </param>
	/// <param name="settings">
	/// The <see cref="DefaultModelResponseValidatorSettings"/> configuration used by the validator.
	/// </param>
	/// <returns>
	/// The <see cref="ValidationPipelineBuilder"/> so that additional stages can be chained.
	/// </returns>
	public static ValidationPipelineBuilder AddModelResponseValidator(
		this ValidationPipelineBuilder builder,
		DefaultModelResponseValidatorSettings settings)
	{
		return builder.AddStage<DefaultModelResponseValidator>((sp, key) =>
			new DefaultModelResponseValidator(
				settings,
				sp.GetService<ILoggerFactory>()));
	}

	/// <summary>
	/// Adds the <see cref="DefaultModelResponseValidator"/> to the validation pipeline.
	/// </summary>
	/// <remarks>
	/// This overload provides the simplest way to enable model response validation. Default configuration is used.
	/// </remarks>
	/// <param name="builder">
	/// The <see cref="ValidationPipelineBuilder"/> used to construct the validation pipeline.
	/// </param>
	/// <returns>
	/// The <see cref="ValidationPipelineBuilder"/> so that additional stages can be chained.
	/// </returns>
	public static ValidationPipelineBuilder AddModelResponseValidator(
		this ValidationPipelineBuilder builder)
	{
		return builder.AddModelResponseValidator(new DefaultModelResponseValidatorSettings());
	}
}
