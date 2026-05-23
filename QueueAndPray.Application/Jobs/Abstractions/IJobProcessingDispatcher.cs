using QueueAndPray.Application.Jobs.Events;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobProcessingDispatcher
{
    Task DispatchAsync(
        IJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken);
}