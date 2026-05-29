using Microsoft.Extensions.Logging;
using QueueAndPray.Application.Common.Resilience;

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

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, int, CancellationToken, Task>? onRetry,
        Func<Exception, int, CancellationToken, Task>? onFinalFailure,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await operation(cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                if (attempt == MaxAttempts)
                {
                    _logger.LogError(
                        ex,
                        "Operation failed on final attempt {Attempt}.",
                        attempt);

                    if (onFinalFailure is not null)
                    {
                        await onFinalFailure(
                            ex,
                            attempt,
                            cancellationToken);
                    }

                    throw;
                }

                _logger.LogWarning(
                    ex,
                    "Operation failed on attempt {Attempt}. Retrying...",
                    attempt);

                if (onRetry is not null)
                {
                    await onRetry(
                        ex,
                        attempt,
                        cancellationToken);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(attempt),
                    cancellationToken);
            }
        }
    }
}