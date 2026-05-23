using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobProcessor<in TEvent>
{
    Task ProcessAsync(TEvent jobQueuedEvent, CancellationToken cancellationToken);
}