using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prompt2Plot.Defaults;
using Prompt2Plot.InMemory;

namespace Prompt2Plot;

public static class BuilderExtensions
{
	public static Prompt2PlotBuilder UseInMemoryWorkItemRepository(
		this Prompt2PlotBuilder builder,
		IServiceCollection services,
		Func<IServiceProvider, InMemoryWorkItemRepositorySettings> settingsProvider)
	{
		services.AddInMemoryWorkItemRepository(settingsProvider);

		return builder.WithWorkItemRepository<InMemoryWorkItemRepository>();
	}

	public static Prompt2PlotBuilder UseInMemoryWorkItemRepository(
		this Prompt2PlotBuilder builder,
		IServiceCollection services)
	{
		services.AddInMemoryWorkItemRepository();

		return builder.WithWorkItemRepository<InMemoryWorkItemRepository>();
	}

	public static PromptPipelineBuilder AddInitialPromptStage(
		this PromptPipelineBuilder builder,
		Func<IServiceProvider, object?, DefaultInitialPromptStageSettings> settingsProvider)
	{
		return builder.AddStage<DefaultInitialPromptStage>((sp, key) =>
			new DefaultInitialPromptStage(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	public static PromptPipelineBuilder AddInitialPromptStage(
		this PromptPipelineBuilder builder,
		DefaultInitialPromptStageSettings settings)
	{
		return builder.AddStage<DefaultInitialPromptStage>((sp, _) =>
			new DefaultInitialPromptStage(
				settings,
				sp.GetService<ILoggerFactory>()));
	}

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

	public static ValidationPipelineBuilder AddModelResponseValidator(
		this ValidationPipelineBuilder builder,
		Func<IServiceProvider, object?, DefaultModelResponseValidatorSettings> settingsProvider)
	{
		return builder.AddStage<DefaultModelResponseValidator>((sp, key) =>
			new DefaultModelResponseValidator(
				settingsProvider(sp, key),
				sp.GetService<ILoggerFactory>()));
	}

	public static ValidationPipelineBuilder AddModelResponseValidator(
		this ValidationPipelineBuilder builder,
		DefaultModelResponseValidatorSettings settings)
	{
		return builder.AddStage<DefaultModelResponseValidator>((sp, key) =>
			new DefaultModelResponseValidator(
				settings,
				sp.GetService<ILoggerFactory>()));
	}

	public static ValidationPipelineBuilder AddModelResponseValidator(
		this ValidationPipelineBuilder builder)
	{
		return builder.AddModelResponseValidator(new DefaultModelResponseValidatorSettings());
	}
}
