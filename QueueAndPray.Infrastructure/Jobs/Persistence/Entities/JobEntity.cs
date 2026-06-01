using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.Entities;

public class JobEntity
{
    public Guid Id { get; set; }

    public string Description { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public JobType Type { get; set; }

    public JobStatus Status { get; set; }

    public string? Result { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public int RetryCount { get; set; }

    public DateTime? FirstFailedAtUtc { get; set; }

    public DateTime? DeadLetteredAtUtc { get; set; }
    public List<JobStatusHistoryEntity> StatusHistory { get; set; } = [];
}
