using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Messaging;

namespace QueueAndPray.Api.Workers;

public class JobProcessingEventConsumer : BackgroundService
{
    private readonly InMemoryJobStatusQueue _jobStatusQueue;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobProcessingEventConsumer> _logger;

    public JobProcessingEventConsumer(
        InMemoryJobStatusQueue jobStatusQueue,
        IJobRepository jobRepository,
        ILogger<JobProcessingEventConsumer> logger)
    {
        _jobStatusQueue = jobStatusQueue;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobStatusEvent in _jobStatusQueue.Reader.ReadAllAsync(stoppingToken))
        {
            switch (jobStatusEvent)
            {
                case JobProcessedEvent processed:
                    await _jobRepository.UpdateStatusAsync(
                        processed.JobId,
                        JobStatus.Completed,
                        "Completed",
                        stoppingToken);

                    _logger.LogInformation("Job {JobId} status changed to Completed", processed.JobId);
                    break;

                case JobFailedEvent failed:
                    await _jobRepository.UpdateStatusAsync(
                        failed.JobId,
                        JobStatus.Failed,
                        failed.Reason,
                        stoppingToken);

                    _logger.LogWarning(
                        "Job {JobId} status changed to Failed. Reason: {Reason}",
                        failed.JobId,
                        failed.Reason);
                    break;

                default:
                    _logger.LogWarning(
                        "Unknown job status event type: {EventType}",
                        jobStatusEvent.GetType().Name);
                    break;
            }
        }
    }
}
