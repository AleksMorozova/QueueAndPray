using QueueAndPray.Application.Common.Resilience;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Orchestration;

public class EmailJobOrchestrator : IEmailJobOrchestrator
{
    private readonly IEmailJobProcessor _processor;
    private readonly IRetryPolicyExecutor _retryPolicyExecutor;
    private readonly IJobRepository _jobRepository;
    private readonly IJobStatusPublishers _jobStatusDispatcher;

    public EmailJobOrchestrator(
        IEmailJobProcessor processor,
        IRetryPolicyExecutor retryPolicyExecutor,
        IJobStatusPublishers jobStatusDispatcher,
        IJobRepository jobRepository)
    {
        _processor = processor;
        _retryPolicyExecutor = retryPolicyExecutor;
        _jobStatusDispatcher = jobStatusDispatcher;
        _jobRepository = jobRepository;
    }

    public async Task ProcessAsync(
        JobQueuedEvent payload,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(
            payload.JobId,
            cancellationToken);

        if (job is null)
        {
            return;
        }

        try
        {
            job.StartProcessing();

            await _jobRepository.SaveAsync(job, cancellationToken);

            var result = await _retryPolicyExecutor.ExecuteAsync(
                operation: ct => _processor.SendEmailAsync(payload, ct),
                onRetry: (ex, attempt, ct) =>
                {
                    job.TrackRetryAttempt(attempt, ex.Message);
                    return _jobRepository.SaveAsync(job, ct);
                },
                cancellationToken: cancellationToken);

            if (result.IsSuccess)
            {
                await _jobStatusDispatcher.DispatchAsync(payload, Domain.Jobs.JobStatus.Completed, null, cancellationToken);

                //job.Complete("Email sent");
            }
            else
            {
                await _jobStatusDispatcher.DispatchAsync(payload, Domain.Jobs.JobStatus.DeadLettered, "Email processing failed", cancellationToken);

                //job.DeadLetter(result.ErrorMessage ?? "Email processing failed");
            }

            //await _jobRepository.SaveAsync(job, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobStatusDispatcher.DispatchAsync(payload, Domain.Jobs.JobStatus.Failed, "Email processing failed. " + ex.Message, cancellationToken);

            //job.Fail(ex.Message);

            //await _jobRepository.SaveAsync(job, cancellationToken);
        }
    }
}