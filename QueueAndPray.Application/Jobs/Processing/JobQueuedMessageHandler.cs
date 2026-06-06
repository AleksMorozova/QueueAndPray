using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;
using System.Text.Json;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class JobQueuedMessageHandler : IJobQueuedMessageHandler
{
    private readonly IInboxRepository _inboxRepository;
    private readonly IEmailJobOrchestrator _orchestrator;
    private readonly IUnitOfWork _unitOfWork;

    public JobQueuedMessageHandler(IInboxRepository inboxRepository,
        IEmailJobOrchestrator orchestrator,
        IUnitOfWork unitOfWork)
    {
        _inboxRepository = inboxRepository;
        _orchestrator = orchestrator;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<JobQueuedEvent>(message)
            ?? throw new InvalidOperationException(
                "RabbitMQ email job message is empty. QueueAndPray is confused.");

        if (await _inboxRepository.ExistsAsync(payload.MessageId, cancellationToken))
        {
            return;
        }

        await _orchestrator.ProcessAsync(payload, cancellationToken);

        await _inboxRepository.AddAsync(InboxMessage.Create(payload.MessageId), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}