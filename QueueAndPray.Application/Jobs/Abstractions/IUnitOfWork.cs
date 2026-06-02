namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
