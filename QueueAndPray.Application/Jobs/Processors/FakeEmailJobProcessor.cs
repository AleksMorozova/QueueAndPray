using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Processors;

public class FakeEmailJobProcessor : IEmailJobProcessor
{
    public async Task SendEmailAsync(
        JobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Fake email sent successfully for job {jobQueuedEvent.JobId}");
    }
}
