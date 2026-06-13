using Microsoft.Extensions.Logging;
using QueueAndPray.Abstractions.Common.Resilience;

namespace QueueAndPray.Infrastructure.Resilience;

public sealed class RetryPolicyExecutor : IRetryPolicyExecutor
{
    private const int MaxAttempts = 3;

    private readonly ILogger<RetryPolicyExecutor> _logger;

    public RetryPolicyExecutor(
        ILogger<RetryPolicyExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<RetryExecutionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, int, CancellationToken, Task>? onRetry,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await operation(cancellationToken);
                return new RetryExecutionResult(true);
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt <= MaxAttempts && onRetry is not null)
                {
                    await onRetry(ex, attempt, cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
                }
            }
        }

        return new RetryExecutionResult(
            false,
            lastException?.Message ?? "Max retries exceeded",
            lastException);
    }
}