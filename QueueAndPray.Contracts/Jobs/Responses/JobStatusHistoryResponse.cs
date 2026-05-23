using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Contracts.Jobs.Responses;

public class JobStatusHistoryResponse
{
    public JobStatus Status { get; set; } = default!;
    public string? Result { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}
