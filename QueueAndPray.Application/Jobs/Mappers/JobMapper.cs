using QueueAndPray.Contracts.Jobs.Responses;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Mappers;

internal static class JobMapper
{
    public static JobDetailedResponse ToDetailedResponse(this Job job)
    {
        if (job is null) return null!;

        var createdUtc = DateTime.SpecifyKind(job.CreatedAtUtc, DateTimeKind.Utc);

        return new JobDetailedResponse
        {
            JobId = job.Id,
            Description = job.Description,
            Type = job.Type,
            Status = job.Status,
            Payload = job.Payload,
            Result = job.Result,
            RetryCount = 0,
            CreatedAt = new DateTimeOffset(createdUtc),
            CompletedAt = null,
            StatusHistory = job.StatusHistory
               .OrderBy(x => x.ChangedAtUtc)
               .Select(x => new JobStatusHistoryResponse
               {
                   Status = x.Status,
                   Result = x.Result,
                   ChangedAt = new DateTimeOffset(
                       DateTime.SpecifyKind(x.ChangedAtUtc, DateTimeKind.Utc))
               })
               .ToList()
        };
    }

    public static JobResponse ToJobResponse(this Job job)
    {
        if (job is null) return null!;

        var createdUtc = DateTime.SpecifyKind(job.CreatedAtUtc, DateTimeKind.Utc);

        return new JobResponse
        {
            JobId = job.Id,
            Description = job.Description,
            Type = job.Type,
            Status = job.Status,
            Result = job.Result
        };
    }
}
