using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;

namespace QueueAndPray.PdfWorker.Processing;

public sealed class PdfJobProcessor
{
    public Task GenerateAsync(
        JobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        if (jobQueuedEvent.Payload.Contains(
                "error",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("PDF generation failed. The payload contained forbidden dark magic.");
        }

        return Task.CompletedTask;
    }
}
