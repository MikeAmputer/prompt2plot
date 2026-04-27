namespace BlazorDemo.Services;

public class PromptExecutorController
{
	public const string FakeGptFlowKey = "fake-gpt-flow";
	public const string ChatGptFlowKey = "chat-gpt-flow";

	private string _currentFlowKey = FakeGptFlowKey;

	public string CurrentFlowKey => ApiKey == null || Model == null ? FakeGptFlowKey : _currentFlowKey;

	public string? ApiKey { get; private set; } = null;
	public string? Model { get; private set; } = null;

	public void SetupFakeGpt()
	{
		ApiKey = null;
		Model = null;

		_currentFlowKey = FakeGptFlowKey;
	}

	public void SetupChatGpt(string apiKey, string model)
	{
		ApiKey = apiKey;
		Model = model;

		_currentFlowKey = ChatGptFlowKey;
	}
}
