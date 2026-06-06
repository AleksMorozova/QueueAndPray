using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Entities;

public class JobStatusHistoryEntity
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public JobEntity Job { get; set; } = default!;

    public JobStatus Status { get; set; }

    public string? Result { get; set; }

    public DateTime ChangedAtUtc { get; set; }
}
