using QueueAndPray.Contracts.Common;

namespace QueueAndPray.Contracts.Jobs.Responses;

// Detailed representation of a job
public class JobDetailedResponse
{
    public Guid JobId { get; init; }

    public string Description { get; init; } = default!;

    public string JobType { get; init; } = default!;

    public string Status { get; init; } = default!;

    public string Payload { get; init; } = default!;

    public string Reason { get; init; } = default!;

    public int RetryCount { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    // Add further detailed fields here as needed
}