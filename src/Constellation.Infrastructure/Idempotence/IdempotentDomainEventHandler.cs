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

    public IdempotentDomainEventHandler(AppDbContext dbContext, INotificationHandler<TDomainEvent> decorated, ILogger logger)
    {
        _dbContext = dbContext;
        _decorated = decorated;
        _logger = logger.ForContext(_decorated.GetType());
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        string consumer = _decorated.GetType().Name;

        if (await _dbContext.Set<OutboxMessageConsumer>()
                .AnyAsync(
                    outboxMessageConsumer =>
                        outboxMessageConsumer.Id == notification.Id.Value &&
                        outboxMessageConsumer.Name == consumer,
                    cancellationToken))
        {
            _logger
                .ForContext("Notification", notification.GetType().Name)
                .ForContext("Notification Id", notification.Id, true)
                .ForContext("Consumer", _decorated.GetType())
                .Information("SKIPPED: {notification_type} WITH: Already marked completed in database", notification.GetType().Name);

            return;
        }

        await _decorated.Handle(notification, cancellationToken);
        
        _dbContext.Set<OutboxMessageConsumer>()
            .Add(new OutboxMessageConsumer
            {
                Id = notification.Id.Value,
                Name = consumer
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
