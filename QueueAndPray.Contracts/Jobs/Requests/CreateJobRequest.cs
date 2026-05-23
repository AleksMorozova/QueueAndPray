using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Contracts.Jobs.Requests;

public class CreateJobRequest
{
    public string Description { get; init; } = default!;

    public JobType Type { get; init; } = default!;

    public string Payload { get; init; } = default!;
}
