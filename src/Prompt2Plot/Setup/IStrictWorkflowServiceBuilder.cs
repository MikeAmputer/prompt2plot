namespace Prompt2Plot;

public interface IStrictWorkflowServiceBuilder
{
	IStrictWorkflowServiceBuilder WithPromptPipeline(Action<PromptPipelineBuilder> setup);
	IStrictWorkflowServiceBuilder WithValidationPipeline(Action<ValidationPipelineBuilder> setup);
}
