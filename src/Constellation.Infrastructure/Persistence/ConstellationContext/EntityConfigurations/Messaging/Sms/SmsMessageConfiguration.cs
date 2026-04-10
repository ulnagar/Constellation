namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Sms;

using Constellation.Core.ValueObjects;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class SmsMessageConfiguration : IEntityTypeConfiguration<SmsMessage>
{
    public void Configure(EntityTypeBuilder<SmsMessage> builder)
    {
        builder.ToTable("Sms", "Messages");

        builder
            .HasKey(message => message.Id);

        builder
            .Property(message => message.Id)
            .HasConversion(
                id => id.Value,
                value => SmsId.FromValue(value));

        builder
            .OwnsOne(message => message.Sender, owned =>
            {
                owned.WithOwner();

                owned
                    .Property(r => r.Name)
                    .HasColumnName($"{nameof(SmsMessage.Sender)}_{nameof(SmsRecipient.Name)}")
                    .IsRequired()
                    .HasMaxLength(200);

                owned
                    .Property(r => r.Number)
                    .HasColumnName($"{nameof(SmsMessage.Sender)}_{nameof(SmsRecipient.Number)}")
                    .IsRequired()
                    .HasMaxLength(320);

                owned
                    .Ignore(r => r.Value);

                owned.HasIndex(r => r.Number);
            }); 

        builder
            .OwnsOne(message => message.Recipient, owned =>
            {
                owned.WithOwner();

                owned
                    .Property(r => r.Name)
                    .HasColumnName($"{nameof(SmsMessage.Recipient)}_{nameof(SmsRecipient.Name)}")
                    .IsRequired()
                    .HasMaxLength(200);

                owned
                    .Property(r => r.Number)
                    .HasColumnName($"{nameof(SmsMessage.Recipient)}_{nameof(SmsRecipient.Number)}")
                    .IsRequired()
                    .HasMaxLength(320);

                owned
                    .Ignore(r => r.Value);

                owned.HasIndex(r => r.Number);
            });

        builder
            .Property(message => message.Message)
            .IsRequired()
            .HasMaxLength(1600); // 10 x 160 char SMS segments

        builder
            .Property(message => message.Direction)
            .HasConversion(
                direction => direction.Value,
                value => MessageDirection.FromValue(value));

        builder
            .Property(message => message.Status)
            .HasConversion(
                status => status.Value,
                value => MessageStatus.FromValue(value));
        
        // Indexes to support the common lookup patterns discussed earlier
        builder
            .HasIndex(message => message.SmsGlobalId);

        builder
            .HasIndex(message => message.OutgoingId);

        builder
            .HasIndex(message => new
            {
                message.Status, 
                message.Direction
            }); // status filtering
    }
}
