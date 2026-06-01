using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Publishers;

public class JobStatusPublisher : IJobStatusPublishers
{
    private readonly IJobStatusQueue _jobStatusQueue;

    public JobStatusPublisher(IJobStatusQueue jobStatusQueue)
    {
        _jobStatusQueue = jobStatusQueue;
    }

    public async Task DispatchAsync(JobQueuedEvent jobQueuedEvent, JobStatus status, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            var jobStatusEvent = new JobStatusEvent
            {
                JobId = jobQueuedEvent.JobId,
                Status = status,
                Type = JobType.Email,
                ProceedsAtUtc = DateTime.UtcNow,
                Reason = reason
            };

            await _jobStatusQueue.PublishAsync(jobStatusEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AppException("job_status_not_published", $"Job '{jobQueuedEvent.JobId}' was lost somewhere between hope and queue. Base exception: {ex.Message}");
        }
    }
}
