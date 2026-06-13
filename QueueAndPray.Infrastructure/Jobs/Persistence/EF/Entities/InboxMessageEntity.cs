namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Entities;

public class InboxMessageEntity
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public DateTime ProcessedAtUtc { get; set; }
}
