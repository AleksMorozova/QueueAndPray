using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.EmailWorker.Processing;

public sealed class FakeEmailJobProcessor : IEmailJobProcessor
{
    public Task SendEmailAsync(JobQueuedEvent jobQueuedEvent, CancellationToken cancellationToken)
    {
        if (jobQueuedEvent.Payload.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The email vanished into the messaging abyss.");
        }

        Console.WriteLine($"Fake email sent successfully for job {jobQueuedEvent.JobId}");

        return Task.CompletedTask;
    }
}
