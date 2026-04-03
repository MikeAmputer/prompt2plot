namespace Prompt2Plot;

internal static class ExceptionExtensions
{
	public static bool IsCausedBy(this Exception exception, CancellationToken cancellationToken)
	{
		return exception is OperationCanceledException oce && oce.CancellationToken == cancellationToken;
	}
}
