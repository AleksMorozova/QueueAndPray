using Microsoft.Extensions.Logging;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class ReportGenerationJobProcessor
    : IJobProcessor<ReportGenerationJobQueuedEvent>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobStatusQueue _jobStatusQueue;
    private readonly ILogger<ReportGenerationJobProcessor> _logger;

    public ReportGenerationJobProcessor(
        IJobRepository jobRepository,
        IJobStatusQueue jobStatusQueue,
        ILogger<ReportGenerationJobProcessor> logger)
    {
        _jobRepository = jobRepository;
        _jobStatusQueue = jobStatusQueue;
        _logger = logger;
    }

    public async Task ProcessAsync(
        ReportGenerationJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating report for job {JobId}",
            jobQueuedEvent.JobId);

        await _jobRepository.UpdateStatusAsync(
            jobQueuedEvent.JobId,
            JobStatus.Processing,
            null,
            cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

        var isSuccess = Random.Shared.Next(0, 2) == 1;

        if (isSuccess)
        {
            await _jobStatusQueue.PublishProcessedAsync(
                new JobProcessedEvent
                {
                    JobId = jobQueuedEvent.JobId,
                    //Result = "Report generated successfully",
                    ProcessedAtUtc = DateTime.UtcNow
                },
                cancellationToken);

            return;
        }

        await _jobStatusQueue.PublishFailedAsync(
            new JobFailedEvent
            {
                JobId = jobQueuedEvent.JobId,
                Reason = "Report data aggregation failed",
                FailedAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }
}