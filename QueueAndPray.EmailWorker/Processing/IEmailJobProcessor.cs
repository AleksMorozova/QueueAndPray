using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.EmailWorker.Processing;

public interface IEmailJobProcessor
{
    Task SendEmailAsync(JobQueuedEvent jobQueuedEvent, CancellationToken cancellationToken);
}
