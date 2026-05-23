namespace QueueAndPray.Application.Jobs.Events;

public interface IJobQueuedEvent
{
    Guid JobId { get; }

    DateTime QueuedAtUtc { get; }
}