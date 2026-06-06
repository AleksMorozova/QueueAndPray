using Microsoft.EntityFrameworkCore;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Repositories;

public class EfInboxRepository : IInboxRepository
{
    private readonly QueueAndPrayDbContext _dbContext;

    public EfInboxRepository(
        QueueAndPrayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(
        InboxMessage message,
        CancellationToken cancellationToken)
    {
        _dbContext.InboxMessages.Add(
            message.ToEntity());

        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.InboxMessages
            .AnyAsync(
                x => x.MessageId == messageId,
                cancellationToken);
    }
}
