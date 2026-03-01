namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Sms;

using Application.Domains.Messaging.Sms.Enums;
using Application.Domains.Messaging.Sms.Identifiers;
using Application.Domains.Messaging.Sms.Models;
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
            .Property(message => message.From)
            .IsRequired()
            .HasMaxLength(20);

        builder
            .Property(message => message.To)
            .IsRequired()
            .HasMaxLength(20);

        builder
            .Property(message => message.Message)
            .IsRequired()
            .HasMaxLength(1600); // 10 x 160 char SMS segments

        builder
            .Property(message => message.Direction)
            .HasConversion(
                direction => direction.Value,
                value => SmsDirection.FromValue(value));

        builder
            .Property(message => message.Status)
            .HasConversion(
                status => status.Value,
                value => SmsStatus.FromValue(value));

        // Self-referencing relationship for reply threading
        builder
            .HasOne(message => message.ReplyTo)
            .WithMany(message => message.Replies)
            .HasForeignKey(message => message.ReplyToId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes to support the common lookup patterns discussed earlier
        builder
            .HasIndex(message => message.SmsGlobalId);

        builder
            .HasIndex(message => message.OutgoingId);

        builder
            .HasIndex(message => new
            {
                message.From, 
                message.To, 
                message.CreatedAt
            }); // conversation lookup

        builder
            .HasIndex(message => new
            {
                message.Status, 
                message.Direction
            }); // status filtering
    }
}
