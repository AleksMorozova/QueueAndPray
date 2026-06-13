using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Entities;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Mappers;

public static class InboxMappings
{
    public static InboxMessage ToDomain(this InboxMessageEntity entity)
    {
        return InboxMessage.Restore(
            messageId: entity.MessageId,
            processedAtUtc: entity.ProcessedAtUtc);
    }

    public static InboxMessageEntity ToEntity(this InboxMessage message)
    {
        return new InboxMessageEntity
        {
            MessageId = message.MessageId,
            ProcessedAtUtc = message.ProcessedAtUtc
        };
    }
}