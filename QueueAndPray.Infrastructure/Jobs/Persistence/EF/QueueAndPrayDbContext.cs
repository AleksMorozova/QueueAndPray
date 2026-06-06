using Microsoft.EntityFrameworkCore;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Entities;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF;

public sealed class QueueAndPrayDbContext : DbContext
{
    public QueueAndPrayDbContext(DbContextOptions<QueueAndPrayDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobEntity> Jobs => Set<JobEntity>();
    public DbSet<JobStatusHistoryEntity> JobStatusHistory => Set<JobStatusHistoryEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();
    public DbSet<InboxMessageEntity> InboxMessages => Set<InboxMessageEntity>();
}