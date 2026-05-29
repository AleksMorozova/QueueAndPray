using QueueAndPray.Application.Common.Resilience;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Orchestration;

public sealed class EmailJobOrchestrator : IEmailJobOrchestrator
{
    private readonly IEmailJobProcessor _processor;
    private readonly IRetryPolicyExecutor _retryPolicyExecutor;
    private readonly IJobStatusDispatcher _jobStatusDispatcher;
    private readonly IJobFailureTracker _jobFailureTracker;

    public EmailJobOrchestrator(
        IEmailJobProcessor processor,
        IRetryPolicyExecutor retryPolicyExecutor,
        IJobStatusDispatcher jobStatusDispatcher,
        IJobFailureTracker jobFailureTracker)
    {
        _processor = processor;
        _retryPolicyExecutor = retryPolicyExecutor;
        _jobStatusDispatcher = jobStatusDispatcher;
        _jobFailureTracker = jobFailureTracker;
    }

    public async Task ProcessAsync(JobQueuedEvent payload, CancellationToken cancellationToken)
    {
        try
        {
            await _retryPolicyExecutor.ExecuteAsync(
                operation: ct => _processor.SendEmailAsync(payload, ct),

                onRetry: (ex, attempt, ct) =>
                    _jobFailureTracker.TrackRetryAttemptAsync(
                        payload.JobId,
                        attempt,
                        ex.Message,
                        ct),

                onFinalFailure: (ex, attempt, ct) =>
                    _jobFailureTracker.TrackDeadLetterAsync(
                        payload.JobId,
                        ex.Message,
                        ct),

                cancellationToken: cancellationToken);

            await _jobStatusDispatcher.DispatchAsync(payload, Domain.Jobs.JobStatus.Completed, null, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobStatusDispatcher.DispatchAsync(
                payload,
                Domain.Jobs.JobStatus.Failed,
                ex.Message,
                cancellationToken);
        }
    }
}