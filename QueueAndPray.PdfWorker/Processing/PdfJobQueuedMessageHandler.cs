using System.Text.Json;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.PdfWorker.Processing;

public sealed class PdfJobQueuedMessageHandler
{
    private readonly IInboxRepository _inboxRepository;
    private readonly PdfJobOrchestrator _orchestrator;
    private readonly IUnitOfWork _unitOfWork;

    public PdfJobQueuedMessageHandler(
        IInboxRepository inboxRepository,
        PdfJobOrchestrator orchestrator,
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
            ?? throw new InvalidOperationException("PDF job message is empty.");

        if (payload.Type != JobType.PdfGeneration)
        {
            throw new InvalidOperationException(
                $"PDF worker received job '{payload.JobId}' with type '{payload.Type}'.");
        }

        if (await _inboxRepository.ExistsAsync(payload.MessageId, cancellationToken))
        {
            return;
        }

        await _orchestrator.ProcessAsync(payload, cancellationToken);

        await _inboxRepository.AddAsync(
            InboxMessage.Create(payload.MessageId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
