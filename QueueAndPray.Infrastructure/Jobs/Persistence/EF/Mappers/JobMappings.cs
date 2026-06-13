using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Entities;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Mappers;

public static class JobMappings
{
    public static Job ToDomain(this JobEntity entity)
    {
        return Job.Restore(
            id: entity.Id,
            description: entity.Description,
            payload: entity.Payload,
            type: entity.Type,
            status: entity.Status,
            result: entity.Result,
            retryCount: entity.RetryCount,
            createdAtUtc: entity.CreatedAtUtc,
            updatedAtUtc: entity.UpdatedAtUtc,
            firstFailedAtUtc: entity.FirstFailedAtUtc,
            deadLetteredAtUtc: entity.DeadLetteredAtUtc,
            statusHistory: entity.StatusHistory.OrderBy(x => x.ChangedAtUtc).Select(x => x.ToDomain()).ToList());
    }

    public static JobEntity ToEntity(this Job job)
    {
        return new JobEntity
        {
            Id = job.Id,
            Description = job.Description,
            Payload = job.Payload,
            Type = job.Type,
            Status = job.Status,
            Result = job.Result,
            RetryCount = job.RetryCount,
            CreatedAtUtc = job.CreatedAtUtc,
            UpdatedAtUtc = job.UpdatedAtUtc,
            FirstFailedAtUtc = job.FirstFailedAtUtc,
            DeadLetteredAtUtc = job.DeadLetteredAtUtc,
            StatusHistory = job.StatusHistory.Select(x => x.ToEntity()).ToList()
        };
    }

    public static void UpdateEntity(
        this JobEntity entity,
        Job job)
    {
        entity.Description = job.Description;
        entity.Payload = job.Payload;
        entity.Type = job.Type;
        entity.Status = job.Status;
        entity.Result = job.Result;
        entity.RetryCount = job.RetryCount;
        entity.UpdatedAtUtc = job.UpdatedAtUtc;
        entity.FirstFailedAtUtc = job.FirstFailedAtUtc;
        entity.DeadLetteredAtUtc = job.DeadLetteredAtUtc;
        entity.CompletedAtUtc = job.CompletedAtUtc;
    }
}