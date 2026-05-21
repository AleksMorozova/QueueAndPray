using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Contracts.Common;
using QueueAndPray.Contracts.Jobs.Requests;
using QueueAndPray.Contracts.Jobs.Responses;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueue _jobQueue;

    public JobService(
        IJobRepository jobRepository,
        IJobQueue jobQueue)
    {
        _jobRepository = jobRepository;
        _jobQueue = jobQueue;
    }

    public async Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jobId = Guid.NewGuid();

        var job = new Job
        {
            Id = jobId,
            Payload = request.Payload,
            Status = JobStatus.Queued,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _jobRepository.AddAsync(job, cancellationToken);

        var jobCreatedEvent = new JobQueuedEvent
        {
            JobId = jobId,
            //Name = request.Name,
            Payload = request.Payload,
            //CreatedAtUtc = DateTime.UtcNow
        };

        await _jobQueue.PublishAsync(jobCreatedEvent, cancellationToken);

        var response = new CreateJobResponse()
        {
            JobId = jobId
        };

        return await Task.FromResult(response);
    }

    public async Task<JobDetailedResponse> GetJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        // Return a simple mock detailed job response.
        var response = new JobDetailedResponse
        {
            JobId = result.Id,
            Description = result.Description,
            Status = result.Status.ToString(),
            Payload = result.Payload,
            Reason = result.Reason,
            RetryCount = 0,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            CompletedAt = DateTimeOffset.UtcNow
        };

        return await Task.FromResult(response);
    }

    public Task<PagedResponse<JobDetailedResponse>> GetJobsAsync(GetJobsRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Create a few mock items. In a real implementation this would query a datastore
        // and apply filtering, sorting and paging based on the request.
        var items = new List<JobDetailedResponse>
        {
            new JobDetailedResponse
            {
                JobId = Guid.NewGuid(),
                JobType = "ExampleJob",
                Status = "Pending",
                RetryCount = 0,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                CompletedAt = null
            },
            new JobDetailedResponse
            {
                JobId = Guid.NewGuid(),
                JobType = "ExampleJob",
                Status = "Running",
                RetryCount = 1,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
                CompletedAt = null
            },
            new JobDetailedResponse
            {
                JobId = Guid.NewGuid(),
                JobType = "ExampleJob",
                Status = "Completed",
                RetryCount = 0,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                CompletedAt = DateTimeOffset.UtcNow.AddMinutes(-50)
            }
        };

        var response = new PagedResponse<JobDetailedResponse>
        {
            Items = items.Select(i => new JobDetailedResponse
            {
                JobId = i.JobId,
                JobType = i.JobType,
                Status = i.Status,
                RetryCount = i.RetryCount,
                CreatedAt = i.CreatedAt,
                CompletedAt = i.CompletedAt
            }).ToList(),
            Page = request?.Page ?? 1,
            PageSize = request?.PageSize ?? items.Count,
            TotalCount = items.Count
        };

        return Task.FromResult(response);
    }
}
