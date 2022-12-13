namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Outbox;

using Constellation.Infrastructure.Persistence.ConstellationContext.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);
    }
}
