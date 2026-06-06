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

    public async Task<RetryExecutionResult> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, int, CancellationToken, Task>? onRetry,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await operation(cancellationToken);

                return new RetryExecutionResult(true);
            }
            catch (Exception ex) when (attempt <= MaxAttempts)
            {
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
            catch (Exception ex)
            {
                return new RetryExecutionResult(
                    false,
                    ex.Message);
            }
        }

        return new RetryExecutionResult(
            false,
            "Unknown retry failure");
    }
}