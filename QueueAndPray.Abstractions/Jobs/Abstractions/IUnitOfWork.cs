namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
