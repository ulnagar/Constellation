namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Drafts;

using Converters;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Identifiers;
using Core.Models.Messaging.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class QueuedMessageConfiguration : IEntityTypeConfiguration<QueuedMessage>
{
    public void Configure(EntityTypeBuilder<QueuedMessage> builder)
    {
        builder.ToTable("Queue", "Messages");

        builder
            .HasKey(item => item.Id);

        builder
            .Property(item => item.Id)
            .HasConversion(
                id => id.Value,
                value => QueuedMessageId.FromValue(value));

        builder
            .Property(item => item.Type)
            .HasConversion(
                type => type.Value,
                value => MessageType.FromValue(value));

        builder
            .OwnsOne(item => item.Sender, s =>
            {
                s.Property(sender => sender.Name)
                    .HasColumnName("SenderName");

                s.Property(sender => sender.Destination)
                    .HasColumnName("SenderDestination");
            });

        builder
            .Property(item => item.Priority)
            .HasConversion<string>();

        builder
            .OwnsMany(item => item.Recipients,
                navigation =>
                {
                    navigation
                        .Property(recipient => recipient.Id)
                        .HasConversion(
                            id => id.Value,
                            value => new MessageRecipientId(value));

                    navigation
                        .Property(recipient => recipient.EmailAddress)
                        .HasConversion<EmailAddressConverter>();

                    navigation
                        .Property(recipient => recipient.PhoneNumber)
                        .HasConversion<PhoneNumberConverter>();

                    navigation.ToJson();
                });
    }
}