namespace QueueAndPray.Domain.Jobs;

public class InboxMessage
{
    private InboxMessage()
    {
    }

    public Guid MessageId { get; private set; }

    public DateTime ProcessedAtUtc { get; private set; }

    public static InboxMessage Create(Guid messageId)
    {
        return new InboxMessage
        {
            MessageId = messageId,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }

    public static InboxMessage Restore(
        Guid messageId,
        DateTime processedAtUtc)
    {
        return new InboxMessage
        {
            MessageId = messageId,
            ProcessedAtUtc = processedAtUtc
        };
    }
}