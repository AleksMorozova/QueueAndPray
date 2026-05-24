using Microsoft.EntityFrameworkCore;
using QueueAndPray.Infrastructure.Jobs.Persistence.Entities;

namespace QueueAndPray.Infrastructure.Jobs.Persistence;

public sealed class QueueAndPrayDbContext : DbContext
{
    public QueueAndPrayDbContext(DbContextOptions<QueueAndPrayDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobEntity> Jobs => Set<JobEntity>();

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<JobEntity>(static builder =>
    //    {
    //        builder.ToTable("jobs");

    //        builder.HasKey(x => x.Id);

    //        builder.Property(x => x.Description)
    //            .HasMaxLength(500)
    //            .IsRequired();

    //        builder.Property(x => x.Payload)
    //            .IsRequired();

    //        builder.Property(x => x.Type)
    //            .HasConversion<string>()
    //            .HasMaxLength(100)
    //            .IsRequired();

    //        builder.Property(x => x.Status)
    //            .HasConversion<string>()
    //            .HasMaxLength(100)
    //            .IsRequired();

    //        builder.Property(x => x.Result)
    //            .HasMaxLength(2000);

    //        builder.Property(x => x.CreatedAtUtc)
    //            .IsRequired();

    //        builder.Property(x => x.UpdatedAtUtc);
    //    });
    //}
}