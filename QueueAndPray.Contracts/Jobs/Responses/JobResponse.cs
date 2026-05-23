using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Contracts.Jobs.Responses;

public class JobResponse
{
    public Guid JobId { get; init; }

    public string Description { get; init; } = default!;

    public JobType Type { get; init; } = default!;

    public JobStatus Status { get; init; } = default!;

    public string Result { get; init; } = default!;
}
