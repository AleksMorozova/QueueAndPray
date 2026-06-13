using QueueAndPray.Abstractions.Common.Exceptions;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobStatusEvents;
using QueueAndPray.Abstractions.Messaging;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.Outbox;

public sealed class OutboxJobStatusPublisher : IJobStatusPublisher
{
    private readonly IOutboxRepository _outboxRepository;

    public OutboxJobStatusPublisher(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task PublishAsync(
        Guid jobId,
        JobType type,
        JobStatus status,
        string? reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var jobStatusEvent = new JobStatusEvent
            {
                MessageId = Guid.NewGuid(),
                CorrelationId = jobId,
                JobId = jobId,
                Status = status,
                Type = type,
                ProceedsAtUtc = DateTime.UtcNow,
                Reason = reason ?? string.Empty
            };

            var outboxMessage = OutboxMessage.Create(
                routingKey: JobBindingPatterns.AnyStatus,
                payload: jobStatusEvent);

            await _outboxRepository.AddAsync(
                outboxMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AppException(
                "job_status_not_published",
                $"Job '{jobId}' status was not published. The outbox raised its hand, but the transaction looked away. Base exception: {ex.Message}");
        }
    }
}
