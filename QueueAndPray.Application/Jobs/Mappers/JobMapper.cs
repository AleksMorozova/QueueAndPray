using QueueAndPray.Contracts.Jobs.Responses;
using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Mappers;

internal static class JobMapper
{
    public static JobDetailedResponse ToDetailedResponse(this Job job)
    {
        var createdUtc = DateTime.SpecifyKind(
            job.CreatedAtUtc,
            DateTimeKind.Utc);

        return new JobDetailedResponse
        {
            JobId = job.Id,
            Description = job.Description,
            Type = job.Type,
            Status = job.Status,
            Payload = job.Payload,
            Result = job.Result ?? string.Empty,
            RetryCount = job.RetryCount,
            CreatedAt = new DateTimeOffset(createdUtc),
            CompletedAt = ToDateTimeOffset(job.CompletedAtUtc),
            StatusHistory = job.StatusHistory
                .OrderBy(x => x.ChangedAtUtc)
                .Select(x => new JobStatusHistoryResponse
                {
                    Status = x.Status,
                    Result = x.Result,
                    ChangedAt = ToDateTimeOffset(x.ChangedAtUtc)!.Value
                })
                .ToList()
        };
    }

    public static JobResponse ToJobResponse(this Job job)
    {
        return new JobResponse
        {
            JobId = job.Id,
            Description = job.Description,
            Type = job.Type,
            Status = job.Status,
            Result = job.Result ?? string.Empty
        };
    }

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        return new DateTimeOffset(
            DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
    }
}
