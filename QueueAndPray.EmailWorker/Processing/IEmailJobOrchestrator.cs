using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.EmailWorker.Processing;

public interface IEmailJobOrchestrator
{
    Task ProcessAsync(JobQueuedEvent payload, CancellationToken cancellationToken);
}
