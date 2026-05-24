using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Mappers;
using QueueAndPray.Contracts.Common;
using QueueAndPray.Contracts.Jobs.Requests;
using QueueAndPray.Contracts.Jobs.Responses;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobDispatcher _jobDispatcher;

    public JobService(
        IJobRepository jobRepository,
        IJobDispatcher jobDispatcher)
    {
        _jobRepository = jobRepository;
        _jobDispatcher = jobDispatcher;
    }

    public virtual async Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTime.UtcNow;
        var jobId = Guid.NewGuid();

        var job = new Job
        {
            Id = jobId,
            Description = request.Description,
            Type = request.Type,
            Payload = request.Payload,
            Status = JobStatus.Queued,
            CreatedAtUtc = now
        };

        await _jobRepository.AddAsync(job, cancellationToken);

        await _jobDispatcher.DispatchAsync(job, cancellationToken);

        var response = new CreateJobResponse
        {
            JobId = jobId
        };

        return response;
    }

    public virtual async Task<JobDetailedResponse> GetJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        if (result is null)
            throw new AppException("key_not_found_error", $"Job '{jobId}' was not found. Maybe it was deployed on Friday.");

        var response = result.ToDetailedResponse();

        return response;
    }

    public async Task<PagedResponse<JobResponse>> GetJobsAsync(GetJobsRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jobs = await _jobRepository.GetAllAsync(cancellationToken);

        var response = new PagedResponse<JobResponse>
        {
            Items = jobs.Select(job => job.ToJobResponse()).ToList(),
            Page = request?.Page ?? 1,
            PageSize = request?.PageSize ?? jobs.Count,
            TotalCount = jobs.Count
        };

        return response;
    }
}
