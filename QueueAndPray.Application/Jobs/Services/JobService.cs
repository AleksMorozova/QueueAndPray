using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Application.Jobs.Mappers;
using QueueAndPray.Contracts.Common;
using QueueAndPray.Contracts.Jobs.Requests;
using QueueAndPray.Contracts.Jobs.Responses;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JobService(
        IJobRepository jobRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public virtual async Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken)
    {
        try 
        {
            ArgumentNullException.ThrowIfNull(request);

            cancellationToken.ThrowIfCancellationRequested();

            var now = DateTime.UtcNow;

            var job = new Job(
                description: request.Description,
                payload: request.Payload,
                type: request.Type);

            await _jobRepository.AddAsync(job, cancellationToken);

            var jobQueuedEvent = new JobQueuedEvent
            {
                JobId = job.Id,
                Payload = job.Payload,
                Type = job.Type,
                QueuedAtUtc = DateTime.UtcNow
            };

            var outboxMessage = OutboxMessage.Create(routingKey: "job.queued", payload: jobQueuedEvent);

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // await _integrationEventPublisher.PublishAsync(outboxMessage.RoutingKey, outboxMessage.Payload, cancellationToken);

            var response = new CreateJobResponse
            {
                JobId = job.Id
            };

            return response;
        }
        catch(Exception ex)
        {
            throw new AppException("job_not_created", $"Job was not created. The queue prayed, the outbox believed, but the transaction had other plans. Base exception: {ex.Message}");
        }
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
