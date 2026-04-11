namespace Prompt2Plot;

internal static class TaskExtensions
{
	public static Task DisposeRegistrationOnCompletion(this Task task, CancellationTokenRegistration registration)
	{
		if (!task.IsCompleted)
		{
			return Await(task, registration);
		}

		registration.Dispose();
		return Task.CompletedTask;

		static async Task Await(Task task, CancellationTokenRegistration registration)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			finally
			{
				// ReSharper disable once MethodHasAsyncOverload
				registration.Dispose();
			}
		}
	}
}
