using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IEmailJobProcessor
{
    Task SendEmailAsync(JobQueuedEvent jobQueuedEvent, CancellationToken cancellationToken);
}
