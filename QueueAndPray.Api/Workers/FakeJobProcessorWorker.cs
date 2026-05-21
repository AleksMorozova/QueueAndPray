using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Messaging;

namespace QueueAndPray.Api.Workers;

public class FakeJobProcessorWorker : BackgroundService
{
    private readonly InMemoryJobQueue _jobQueue;
    private readonly IJobStatusQueue _jobStatusQueue;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<FakeJobProcessorWorker> _logger;

    public FakeJobProcessorWorker(
        InMemoryJobQueue jobQueue,
        IJobStatusQueue jobStatusQueue,
        IJobRepository jobRepository,
        ILogger<FakeJobProcessorWorker> logger)
    {
        _jobQueue = jobQueue;
        _jobStatusQueue = jobStatusQueue;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobQueuedEvent in _jobQueue.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Started processing job {JobId}", jobQueuedEvent.JobId);

            await _jobRepository.UpdateStatusAsync(
                jobQueuedEvent.JobId,
                JobStatus.Processing,
                "Processing",
                stoppingToken);

            await Task.Delay(Random.Shared.Next(1000, 4000), stoppingToken);

            var isSuccess = Random.Shared.Next(0, 2) == 1;

            if (isSuccess)
            {
                await _jobStatusQueue.PublishProcessedAsync(
                    new JobProcessedEvent
                    {
                        JobId = jobQueuedEvent.JobId,
                        ProcessedAtUtc = DateTime.UtcNow
                    },
                    stoppingToken);

                _logger.LogInformation("Job {JobId} processed successfully", jobQueuedEvent.JobId);
            }
            else
            {
                await _jobStatusQueue.PublishFailedAsync(
                    new JobFailedEvent
                    {
                        JobId = jobQueuedEvent.JobId,
                        Reason = "Fake processing failed randomly",
                        FailedAtUtc = DateTime.UtcNow
                    },
                    stoppingToken);

                _logger.LogWarning("Job {JobId} failed during fake processing", jobQueuedEvent.JobId);
            }
        }
    }
}