using QueueAndPray.Application.Jobs.Events;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobQueue
{
    Task PublishAsync(JobQueuedEvent jobCreatedEvent, CancellationToken cancellationToken);
}
