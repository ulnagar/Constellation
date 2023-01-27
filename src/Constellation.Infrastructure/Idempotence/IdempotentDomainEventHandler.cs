namespace Constellation.Infrastructure.Idempotence;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Primitives;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Outbox;
using Microsoft.EntityFrameworkCore;
using Serilog;

public sealed class IdempotentDomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated;
    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;

    public IdempotentDomainEventHandler(AppDbContext dbContext, INotificationHandler<TDomainEvent> decorated, Serilog.ILogger logger)
    {
        _dbContext = dbContext;
        _decorated = decorated;
        _logger = logger.ForContext(_decorated.GetType());
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        string consumer = _decorated.GetType().Name;

        _logger.Information("{ID} PROCESSING: {@notification} WITH: {consumer}", notification.Id, notification, consumer);

        if (await _dbContext.Set<OutboxMessageConsumer>()
                .AnyAsync(
                    outboxMessageConsumer =>
                        outboxMessageConsumer.Id == notification.Id &&
                        outboxMessageConsumer.Name == consumer,
                    cancellationToken))
        {
            _logger.Information("{ID} SKIPPED: {@notification} WITH: Already marked completed in database", notification.Id, notification);

            return;
        }

        _logger.Information("{ID} ATTEMPTING: {@notification}", notification.Id, notification);

        await _decorated.Handle(notification, cancellationToken);

        _logger.Information("{ID} FINISHED: {@notification}", notification.Id, notification);

        _dbContext.Set<OutboxMessageConsumer>()
            .Add(new OutboxMessageConsumer
            {
                Id = notification.Id,
                Name = consumer
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
