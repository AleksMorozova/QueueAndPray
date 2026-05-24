using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;

namespace QueueAndPray.Application.Jobs.Processors;

public sealed class JobStatusProcessor : IJobStatusProcessor
{
    private readonly IJobRepository _jobRepository;

    public JobStatusProcessor(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task DispatchAsync(
        JobStatusEvent jobStatusEvent,
        CancellationToken cancellationToken)
    {

        await _jobRepository.UpdateStatusAsync(
            jobStatusEvent.JobId,
            jobStatusEvent.Status,
            jobStatusEvent.Reason,
            cancellationToken);
    }
}