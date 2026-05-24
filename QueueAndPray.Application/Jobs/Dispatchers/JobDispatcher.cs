using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Dispatchers;

public sealed class JobDispatcher : IJobDispatcher
{
    private readonly IJobQueue _jobQueue;

    public JobDispatcher(IJobQueue jobQueue)
    {
        _jobQueue = jobQueue;
    }

    public async Task DispatchAsync(
        Job job,
        CancellationToken cancellationToken)
    {
        try
        {
            var jobQueuedEvent = new JobQueuedEvent
            {
                JobId = job.Id,
                Payload = job.Payload,
                Type = job.Type,
                QueuedAtUtc = DateTime.UtcNow
            };

            await _jobQueue.PublishAsync(
                jobQueuedEvent,
                cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AppException("job_not_published", $"Job '{job.Id}' was lost somewhere between hope and queue. Base exception: {ex.Message}");
        }
    }
}