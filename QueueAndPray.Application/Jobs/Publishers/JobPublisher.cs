using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Publishers;

public sealed class JobPublisher : IJobPublisher
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public JobPublisher(IIntegrationEventPublisher integrationEventPublisher)
    {
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task DispatchAsync(Job job, CancellationToken cancellationToken)
    {
        try
        {
            if (!JobStatus.Completed.Equals(job.Status))
            {    
                var jobQueuedEvent = new JobQueuedEvent
                {
                    JobId = job.Id,
                    Payload = job.Payload,
                    Type = job.Type,
                    QueuedAtUtc = DateTime.UtcNow
                };

                //await _integrationEventPublisher.PublishAsync(
                //    jobQueuedEvent,
                //    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            throw new AppException("job_not_published", $"Job '{job.Id}' was lost somewhere between hope and queue. Base exception: {ex.Message}");
        }
    }
}