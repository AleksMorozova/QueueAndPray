using Microsoft.Extensions.Logging;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class EmailJobProcessor
    : IJobProcessor<EmailJobQueuedEvent>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobStatusQueue _jobStatusQueue;
    private readonly ILogger<EmailJobProcessor> _logger;

    public EmailJobProcessor(
        IJobRepository jobRepository,
        IJobStatusQueue jobStatusQueue,
        ILogger<EmailJobProcessor> logger)
    {
        _jobRepository = jobRepository;
        _jobStatusQueue = jobStatusQueue;
        _logger = logger;
    }

    public async Task ProcessAsync(
        EmailJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing email job {JobId}",
            jobQueuedEvent.JobId);

        await _jobRepository.UpdateStatusAsync(
            jobQueuedEvent.JobId,
            JobStatus.Processing,
            null,
            cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        var isSuccess = Random.Shared.Next(0, 2) == 1;

        if (isSuccess)
        {
            await _jobStatusQueue.PublishProcessedAsync(
                new JobProcessedEvent
                {
                    JobId = jobQueuedEvent.JobId,
                    // Reason = "Email sent successfully",
                    ProcessedAtUtc = DateTime.UtcNow
                },
                cancellationToken);

            return;
        }

        await _jobStatusQueue.PublishFailedAsync(
            new JobFailedEvent
            {
                JobId = jobQueuedEvent.JobId,
                Reason = "SMTP server connection timeout",
                FailedAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }
}