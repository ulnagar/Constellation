namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.EmergencyConsole;

using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Enums;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MessageEventRecipientConfiguration : IEntityTypeConfiguration<MessageEventRecipient>
{
    public void Configure(EntityTypeBuilder<MessageEventRecipient> builder)
    {
        builder.ToTable("MessageRecipients", "EmergencyConsole");

        builder
            .HasKey(recipient => recipient.Id);

        builder
            .Property(recipient => recipient.Id)
            .HasConversion(
                id => id.Value,
                value => MessageId.FromValue(value));

        builder
            .Property(recipient => recipient.Type)
            .IsRequired()
            .HasConversion(
                type => type.Value,
                value => MessageType.FromValue(value));

        builder
            .Property(recipient => recipient.Status)
            .IsRequired()
            .HasConversion(
                status => status.Value,
                value => MessageStatus.FromValue(value));
    }
}