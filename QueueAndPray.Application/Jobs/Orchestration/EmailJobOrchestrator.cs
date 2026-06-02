using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Common.Resilience;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Orchestration;

public class EmailJobOrchestrator : IEmailJobOrchestrator
{
    private readonly IEmailJobProcessor _processor;
    private readonly IRetryPolicyExecutor _retryPolicyExecutor;
    private readonly IJobRepository _jobRepository;
    private readonly IJobStatusPublisher _jobStatusPublisher;

    public EmailJobOrchestrator(
        IEmailJobProcessor processor,
        IRetryPolicyExecutor retryPolicyExecutor,
        IJobStatusPublisher jobStatusPublisher,
        IJobRepository jobRepository)
    {
        _processor = processor;
        _retryPolicyExecutor = retryPolicyExecutor;
        _jobStatusPublisher = jobStatusPublisher;
        _jobRepository = jobRepository;
    }

    public async Task ProcessAsync(
        JobQueuedEvent payload,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(payload.JobId, cancellationToken);

        if (job is null)
        {
            throw new AppException("job_not_found_error", $"Job '{payload.JobId}' was not found. The queue swears it existed five minutes ago.");
        }

        if (!job.CanBeProcessed())
        {
            return;
        }

        try
        {
            job.StartProcessing();

            await _jobRepository.SaveAsync(job, cancellationToken);

            var result = await _retryPolicyExecutor.ExecuteAsync(
                operation: ct => _processor.SendEmailAsync(payload, ct),
                onRetry: async (ex, attempt, ct) =>
                {
                    job.TrackRetryAttempt(attempt, ex.Message);
                    await _jobRepository.SaveAsync(job, ct);
                },
                cancellationToken: cancellationToken);

            if (result.IsSuccess)
            {
                await _jobStatusPublisher.PublishAsync(
                    payload.JobId,
                    payload.Type,
                    JobStatus.Completed,
                    "Email sent",
                    cancellationToken);

                //job.Complete("Email sent");
            }
            else
            {
                await _jobStatusPublisher.PublishAsync(
                    payload.JobId,
                    payload.Type,
                    JobStatus.DeadLettered,
                    "Email processing failed. " + result?.FinalException?.Message,
                    cancellationToken);

                //job.DeadLetter(result.ErrorMessage ?? "Email processing failed");
            }

            //await _jobRepository.SaveAsync(job, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobStatusPublisher.PublishAsync(
                payload.JobId,
                payload.Type,
                JobStatus.Failed,
                "Email processing failed. " + ex.Message,
                cancellationToken);


            //job.Fail(ex.Message);

            //await _jobRepository.SaveAsync(job, cancellationToken);
        }
    }
}