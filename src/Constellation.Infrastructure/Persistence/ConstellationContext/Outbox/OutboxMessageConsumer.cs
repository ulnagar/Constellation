namespace Constellation.Infrastructure.Persistence.ConstellationContext.Outbox;

public sealed class OutboxMessageConsumer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
