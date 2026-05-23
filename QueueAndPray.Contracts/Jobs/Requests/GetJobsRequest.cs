using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Contracts.Jobs.Requests;

public class GetJobsRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public JobStatus? Status { get; init; }

    public JobType? JobType { get; init; }

    public DateTimeOffset? CreatedFrom { get; init; }

    public DateTimeOffset? CreatedTo { get; init; }
}