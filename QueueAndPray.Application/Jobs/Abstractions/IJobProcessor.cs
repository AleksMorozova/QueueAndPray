using QueueAndPray.Application.Jobs.Events;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobProcessor<in TEvent>
    where TEvent : IJobQueuedEvent
{
    Task ProcessAsync(TEvent jobQueuedEvent, CancellationToken cancellationToken);
}