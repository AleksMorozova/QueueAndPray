using System.Text.Json;

namespace QueueAndPray.Domain.Jobs;

public class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = default!;

    public string RoutingKey { get; private set; } = default!;

    public string Payload { get; private set; } = default!;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage Create<T>(
        string routingKey,
        T payload)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).Name,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static OutboxMessage Restore(
        Guid id,
        string type,
        string routingKey,
        string payload,
        DateTime createdAtUtc,
        DateTime? publishedAtUtc,
        string? error)
    {
        return new OutboxMessage
        {
            Id = id,
            Type = type,
            RoutingKey = routingKey,
            Payload = payload,
            CreatedAtUtc = createdAtUtc,
            PublishedAtUtc = publishedAtUtc,
            Error = error
        };
    }

    public void MarkAsPublished()
    {
        PublishedAtUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}
