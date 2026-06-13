using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IJobProcessor<in TEvent>
{
    Task ProcessAsync(TEvent jobQueuedEvent, CancellationToken cancellationToken);
}