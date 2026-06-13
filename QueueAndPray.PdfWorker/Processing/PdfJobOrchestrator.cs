using QueueAndPray.Abstractions.Common.Resilience;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;
using QueueAndPray.Abstractions.Common.Exceptions;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.PdfWorker.Processing;

public sealed class PdfJobOrchestrator
{
    private readonly PdfJobProcessor _processor;
    private readonly IRetryPolicyExecutor _retryPolicyExecutor;
    private readonly IJobStatusPublisher _jobStatusPublisher;

    public PdfJobOrchestrator(
        PdfJobProcessor processor,
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
        if (payload.Type != JobType.PdfGeneration)
        {
            throw new AppException(
                "invalid_pdf_job_type",
                $"PDF worker got job '{payload.JobId}' with type '{payload.Type}'. Not my circus, not my PDF.");
        }

        await _jobStatusPublisher.PublishAsync(
            payload.JobId,
            payload.Type,
            JobStatus.Processing,
            "PDF processing started",
            cancellationToken);

        if (payload.Payload.Contains(
                "failed",
                StringComparison.OrdinalIgnoreCase))
        {
            await _jobStatusPublisher.PublishAsync(
                payload.JobId,
                payload.Type,
                JobStatus.Failed,
                "PDF generation was asked to fail. QueueAndPray obeyed dramatically.",
                cancellationToken);

            return;
        }

        var result = await _retryPolicyExecutor.ExecuteAsync(
            operation: ct => _processor.GenerateAsync(payload, ct),
            onRetry: async (ex, attempt, ct) =>
            {
                await _jobStatusPublisher.PublishAsync(
                    payload.JobId,
                    payload.Type,
                    JobStatus.Processing,
                    $"PDF retry attempt {attempt}: {ex.Message}",
                    ct);
            },
            cancellationToken: cancellationToken);

        if (result.IsSuccess)
        {
            await _jobStatusPublisher.PublishAsync(
                payload.JobId,
                payload.Type,
                JobStatus.Completed,
                "PDF generated successfully. The document gods are pleased.",
                cancellationToken);

            return;
        }

        await _jobStatusPublisher.PublishAsync(
            payload.JobId,
            payload.Type,
            JobStatus.DeadLettered,
            "PDF generation failed after retries. The printer demon wins this round. " + result.FinalException?.Message,
            cancellationToken);
    }
}
