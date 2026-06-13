using QueueAndPray.Abstractions.Common.Exceptions;
using QueueAndPray.Abstractions.Common.Resilience;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.EmailWorker.Processing;

public class EmailJobOrchestrator : IEmailJobOrchestrator
{
    private readonly IEmailJobProcessor _processor;
    private readonly IRetryPolicyExecutor _retryPolicyExecutor;
    private readonly IJobStatusPublisher _jobStatusPublisher;

    public EmailJobOrchestrator(
        IEmailJobProcessor processor,
        IRetryPolicyExecutor retryPolicyExecutor,
        IJobStatusPublisher jobStatusPublisher)
    {
        _processor = processor;
        _retryPolicyExecutor = retryPolicyExecutor;
        _jobStatusPublisher = jobStatusPublisher;
    }

    public async Task ProcessAsync(
        JobQueuedEvent payload,
        CancellationToken cancellationToken)
    {
        try
        {
            await _jobStatusPublisher.PublishAsync(
                payload.JobId,
                payload.Type,
                JobStatus.Processing,
                "Email processing started",
                cancellationToken);

            var result = await _retryPolicyExecutor.ExecuteAsync(
                operation: ct => _processor.SendEmailAsync(payload, ct),
                onRetry: async (ex, attempt, ct) =>
                {
                    await _jobStatusPublisher.PublishAsync(
                        payload.JobId,
                        payload.Type,
                        JobStatus.Processing,
                        $"Email retry attempt {attempt}: {ex.Message}",
                        ct);
                },
                cancellationToken: cancellationToken);

            if (result.IsSuccess)
            {
                await _jobStatusPublisher.PublishAsync(
                    payload.JobId,
                    payload.Type,
                    JobStatus.Completed,
                    "Email sent",
                    cancellationToken);
            }
            else
            {
                await _jobStatusPublisher.PublishAsync(
                    payload.JobId,
                    payload.Type,
                    JobStatus.DeadLettered,
                    "Email processing failed. " + result?.FinalException?.Message,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await _jobStatusPublisher.PublishAsync(
                payload.JobId,
                payload.Type,
                JobStatus.Failed,
                "Email processing failed. " + ex.Message,
                cancellationToken);
        }
    }
}
