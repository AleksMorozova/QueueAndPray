using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class JobStatusProcessor : IJobStatusProcessor
{
    private readonly IJobRepository _jobRepository;

    public JobStatusProcessor(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task DispatchAsync(JobStatusEvent jobStatusEvent, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(jobStatusEvent.JobId, cancellationToken);

        if (job is null)
        {
            return;
        }

        job.ApplyExternalStatus(jobStatusEvent.Status, jobStatusEvent.Reason);

        await _jobRepository.SaveAsync(job, cancellationToken);
    }
}