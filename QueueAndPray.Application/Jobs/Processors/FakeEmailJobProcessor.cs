using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.Application.Jobs.Processors;

public sealed class FakeEmailJobProcessor : IEmailJobProcessor
{
    private readonly IJobStatusDispatcher _jobStatusDispatcher;

    public FakeEmailJobProcessor(
        IJobStatusDispatcher jobStatusDispatcher)
    {
        _jobStatusDispatcher = jobStatusDispatcher;
    }

    public async Task SendEmailAsync(
        JobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        if (jobQueuedEvent.Payload.Contains(
                "error",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The email vanished into the messaging abyss.");
        }

        Console.WriteLine($"Fake email sent successfully for job {jobQueuedEvent.JobId}");

        await _jobStatusDispatcher.DispatchAsync(jobQueuedEvent, cancellationToken);
    }
}