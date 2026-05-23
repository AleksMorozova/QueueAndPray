using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Dispatchers;

public sealed class JobDispatcher : IJobDispatcher
{
    private readonly IJobQueue _jobQueue;

    public JobDispatcher(IJobQueue jobQueue)
    {
        _jobQueue = jobQueue;
    }

    public async Task DispatchAsync(
        Job job,
        CancellationToken cancellationToken)
    {
        // Potentially send to different queue
        IJobQueuedEvent jobQueuedEvent = job.Type switch
        {
            JobType.Email => new EmailJobQueuedEvent
            {
                JobId = job.Id,
                Payload = job.Payload,
                QueuedAtUtc = DateTime.UtcNow
            },

            JobType.PdfGeneration => new PdfGenerationJobQueuedEvent
            {
                JobId = job.Id,
                Payload = job.Payload,
                QueuedAtUtc = DateTime.UtcNow
            },

            JobType.ReportGeneration => new ReportGenerationJobQueuedEvent
            {
                JobId = job.Id,
                Payload = job.Payload,
                QueuedAtUtc = DateTime.UtcNow
            },

            _ => throw new NotSupportedException(
                $"Job type {job.Type} is not supported")
        };

        await _jobQueue.PublishAsync(
            jobQueuedEvent,
            cancellationToken);
    }
}