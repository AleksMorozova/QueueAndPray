using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobStatusEvents;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Processing;

public sealed class JobStatusProcessor : IJobStatusProcessor
{
    private readonly IJobRepository _jobRepository;
    private readonly IInboxRepository _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JobStatusProcessor(
        IJobRepository jobRepository,
        IInboxRepository inboxRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _inboxRepository = inboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task DispatchAsync(JobStatusEvent jobStatusEvent, CancellationToken cancellationToken)
    {
        if (await _inboxRepository.ExistsAsync(jobStatusEvent.MessageId, cancellationToken))
        {
            return;
        }

        var job = await _jobRepository.GetByIdAsync(jobStatusEvent.JobId, cancellationToken);

        if (job is null)
        {
            await _inboxRepository.AddAsync(
                InboxMessage.Create(jobStatusEvent.MessageId),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        job.ApplyExternalStatus(jobStatusEvent.Status, jobStatusEvent.Reason);

        await _jobRepository.SaveAsync(job, cancellationToken);

        await _inboxRepository.AddAsync(
            InboxMessage.Create(jobStatusEvent.MessageId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
