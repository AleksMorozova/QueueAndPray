namespace QueueAndPray.Application.Common.Resilience;

public interface IRetryPolicyExecutor
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, int, CancellationToken, Task>? onRetry,
        Func<Exception, int, CancellationToken, Task>? onFinalFailure,
        CancellationToken cancellationToken);
}