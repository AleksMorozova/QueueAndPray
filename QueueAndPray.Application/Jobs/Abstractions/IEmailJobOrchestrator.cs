using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Abstractions;

    public interface IEmailJobOrchestrator
{
    Task ProcessAsync(
        JobQueuedEvent payload,
        CancellationToken cancellationToken);
}