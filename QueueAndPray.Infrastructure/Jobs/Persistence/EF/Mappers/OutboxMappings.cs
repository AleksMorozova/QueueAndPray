using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Mappers;

public static class OutboxMappings
{
    public static OutboxMessage ToDomain(
        this OutboxMessageEntity entity)
    {
        return OutboxMessage.Restore(
            id: entity.Id,
            type: entity.Type,
            routingKey: entity.RoutingKey,
            payload: entity.Payload,
            createdAtUtc: entity.CreatedAtUtc,
            publishedAtUtc: entity.PublishedAtUtc,
            error: entity.Error);
    }

    public static OutboxMessageEntity ToEntity(
        this OutboxMessage message)
    {
        return new OutboxMessageEntity
        {
            Id = message.Id,
            Type = message.Type,
            Payload = message.Payload,
            CreatedAtUtc = message.CreatedAtUtc,
            PublishedAtUtc = message.PublishedAtUtc,
            Error = message.Error,
            RoutingKey = message.RoutingKey
        };
    }

    public static void UpdateEntity(
        this OutboxMessageEntity entity,
        OutboxMessage message)
    {
        entity.PublishedAtUtc = message.PublishedAtUtc;
        entity.Error = message.Error;
    }
}