namespace QueueAndPray.Application.Common.Resilience;

public interface IRetryPolicyExecutor
{
    Task<RetryExecutionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, int, CancellationToken, Task>? onRetry,
        CancellationToken cancellationToken);
}