namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.EmergencyConsole;

using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MessageStatusConfiguration : IEntityTypeConfiguration<MessageStatus>
{
    public void Configure(EntityTypeBuilder<MessageStatus> builder)
    {
        builder.ToTable("MessageStatus", "EmergencyConsole");

        builder
            .HasKey(status => status.Id);

        builder
            .Property(status => status.Id)
            .HasConversion(
                id => id.Value,
                value => MessageId.FromValue(value));

        builder
            .Property(status => status.Type)
            .IsRequired()
            .HasConversion(
                type => type.Value,
                value => MessageType.FromValue(value));
    }
}