namespace QueueAndPray.Infrastructure.Jobs.Persistence.Entities;

public class OutboxMessageEntity
{
    public Guid Id { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public string? Error { get; set; }

    public string RoutingKey { get; set; } = default!;
}
