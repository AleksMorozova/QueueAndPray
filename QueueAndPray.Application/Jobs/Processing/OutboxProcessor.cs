using QueueAndPray.Abstractions.Jobs.Abstractions;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private const int BatchSize = 20;

    private readonly IOutboxRepository _outboxRepository;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public OutboxProcessor(
        IOutboxRepository outboxRepository,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _outboxRepository = outboxRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessPendingAsync(
        CancellationToken cancellationToken)
    {
        var messages = await _outboxRepository.GetPendingAsync(BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublishAsync(
                    message.RoutingKey,
                    message.Payload,
                    cancellationToken);

                message.MarkAsPublished();

                await _outboxRepository.SaveAsync(message, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);

                await _outboxRepository.SaveAsync(message, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}