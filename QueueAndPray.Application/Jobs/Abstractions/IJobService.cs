using QueueAndPray.Contracts.Common;
using QueueAndPray.Contracts.Jobs.Requests;
using QueueAndPray.Contracts.Jobs.Responses;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobService
{
    Task<CreateJobResponse> CreateJobAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken);

    Task<JobDetailedResponse> GetJobAsync(
        Guid jobId,
        CancellationToken cancellationToken);

    Task<PagedResponse<JobDetailedResponse>> GetJobsAsync(
        GetJobsRequest request,
        CancellationToken cancellationToken);
}
