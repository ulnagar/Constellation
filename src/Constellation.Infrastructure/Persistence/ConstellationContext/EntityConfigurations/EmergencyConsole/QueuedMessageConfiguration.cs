namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.EmergencyConsole;

using Converters;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Identifiers;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class QueuedMessageConfiguration : IEntityTypeConfiguration<QueuedMessage>
{
    public void Configure(EntityTypeBuilder<QueuedMessage> builder)
    {
        builder.ToTable("Queue", "EmergencyConsole");

        builder.HasKey(qm => new { qm.EventId, qm.MessageId });

        builder
            .Property(item => item.EventId)
            .HasConversion(
                id => id.Value,
                value => EventId.FromValue(value));

        builder
            .Property(item => item.MessageId)
            .HasConversion(
                id => id.Value,
                value => MessageId.FromValue(value));

        builder
            .Property(item => item.AlertRecipient)
            .IsRequired()
            .HasConversion<JsonColumnConverter<AlertRecipient>>();
    }
}
