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

    public DateTime? LockedUntilUtc { get; private set; }

    public DateTime? NextAttemptAtUtc { get; private set; }

    public int AttemptCount { get; private set; }

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
        DateTime? lockedUntilUtc,
        DateTime? nextAttemptAtUtc,
        int attemptCount,
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
            LockedUntilUtc = lockedUntilUtc,
            NextAttemptAtUtc = nextAttemptAtUtc,
            AttemptCount = attemptCount,
            Error = error
        };
    }

    public void MarkAsClaimed(DateTime lockedUntilUtc)
    {
        LockedUntilUtc = lockedUntilUtc;
    }

    public void MarkAsPublished()
    {
        PublishedAtUtc = DateTime.UtcNow;
        LockedUntilUtc = null;
        NextAttemptAtUtc = null;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        AttemptCount++;
        Error = error;
        LockedUntilUtc = null;
        NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(Math.Min(300, AttemptCount * 10));
    }
}
