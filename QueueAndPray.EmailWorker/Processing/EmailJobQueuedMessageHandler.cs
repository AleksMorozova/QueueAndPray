using System.Text.Json;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.EmailWorker.Processing;

public sealed class EmailJobQueuedMessageHandler : IEmailJobQueuedMessageHandler
{
    private readonly IInboxRepository _inboxRepository;
    private readonly IEmailJobOrchestrator _orchestrator;
    private readonly IUnitOfWork _unitOfWork;

    public EmailJobQueuedMessageHandler(
        IInboxRepository inboxRepository,
        IEmailJobOrchestrator orchestrator,
        IUnitOfWork unitOfWork)
    {
        _inboxRepository = inboxRepository;
        _orchestrator = orchestrator;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(string message, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<JobQueuedEvent>(message)
            ?? throw new InvalidOperationException("Email job message is empty.");

        if (await _inboxRepository.ExistsAsync(payload.MessageId, ct))
        {
            return;
        }

        await _orchestrator.ProcessAsync(payload, ct);

        await _inboxRepository.AddAsync(
            InboxMessage.Create(payload.MessageId),
            ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
