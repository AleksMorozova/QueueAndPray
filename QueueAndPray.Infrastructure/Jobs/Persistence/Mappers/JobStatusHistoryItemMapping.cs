using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.Mappers;

public static class JobStatusHistoryItemMapping
{
    public static JobStatusHistoryItem ToDomain(
    this JobStatusHistoryEntity entity)
    {
        return new JobStatusHistoryItem(
            entity.Id,
            entity.JobId,
            entity.Status,
            entity.Result,
            entity.ChangedAtUtc);
    }

    public static JobStatusHistoryEntity ToEntity(
        this JobStatusHistoryItem item)
    {
        return new JobStatusHistoryEntity
        {
            Id = item.Id,
            JobId = item.JobId,
            Status = item.Status,
            Result = item.Result,
            ChangedAtUtc = item.ChangedAtUtc
        };
    }
}
