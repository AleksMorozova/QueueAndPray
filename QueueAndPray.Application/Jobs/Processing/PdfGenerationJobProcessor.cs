using Microsoft.Extensions.Logging;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class PdfGenerationJobProcessor
    : IJobProcessor<PdfGenerationJobQueuedEvent>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobStatusQueue _jobStatusQueue;
    private readonly ILogger<PdfGenerationJobProcessor> _logger;

    public PdfGenerationJobProcessor(
        IJobRepository jobRepository,
        IJobStatusQueue jobStatusQueue,
        ILogger<PdfGenerationJobProcessor> logger)
    {
        _jobRepository = jobRepository;
        _jobStatusQueue = jobStatusQueue;
        _logger = logger;
    }

    public async Task ProcessAsync(
        PdfGenerationJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating PDF for job {JobId}",
            jobQueuedEvent.JobId);

        await _jobRepository.UpdateStatusAsync(
            jobQueuedEvent.JobId,
            JobStatus.Processing,
            null,
            cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        var isSuccess = Random.Shared.Next(0, 2) == 1;

        if (isSuccess)
        {
            await _jobStatusQueue.PublishProcessedAsync(
                new JobProcessedEvent
                {
                    JobId = jobQueuedEvent.JobId,
                    //Result = "PDF generated successfully",
                    ProcessedAtUtc = DateTime.UtcNow
                },
                cancellationToken);

            return;
        }

        await _jobStatusQueue.PublishFailedAsync(
            new JobFailedEvent
            {
                JobId = jobQueuedEvent.JobId,
                Reason = "PDF template rendering failed",
                FailedAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }
}