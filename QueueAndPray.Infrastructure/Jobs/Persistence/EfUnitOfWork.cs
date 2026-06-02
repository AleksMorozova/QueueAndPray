using QueueAndPray.Application.Jobs.Abstractions;
namespace QueueAndPray.Infrastructure.Jobs.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly QueueAndPrayDbContext _dbContext;

    public EfUnitOfWork(QueueAndPrayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
