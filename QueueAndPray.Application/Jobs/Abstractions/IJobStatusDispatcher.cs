using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobStatusDispatcher
{
    Task DispatchAsync(JobQueuedEvent jobQueuedEvent, JobStatus status, string? reason, CancellationToken cancellationToken);
}
