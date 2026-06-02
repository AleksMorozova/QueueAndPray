using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;
using QueueAndPray.Application.Jobs.Messaging;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Publishers;

public class JobStatusPublisher : IJobStatusPublisher
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JobStatusPublisher(
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
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
                JobId = jobId,
                Status = status,
                Type = type,
                ProceedsAtUtc = DateTime.UtcNow,
                Reason = reason
            };

            var outboxMessage = OutboxMessage.Create(
                routingKey: RoutingKeys.JobStatus,
                payload: jobStatusEvent);

            await _outboxRepository.AddAsync(
                outboxMessage,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AppException(
                "job_status_not_published",
                $"Job '{jobId}' status was not published. The outbox raised its hand, but the transaction looked away. Base exception: {ex.Message}");
        }
    }
}