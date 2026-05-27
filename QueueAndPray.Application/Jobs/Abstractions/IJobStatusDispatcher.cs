using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobStatusDispatcher
{
    Task DispatchAsync(JobQueuedEvent jobQueuedEvent, CancellationToken cancellationToken);
}
