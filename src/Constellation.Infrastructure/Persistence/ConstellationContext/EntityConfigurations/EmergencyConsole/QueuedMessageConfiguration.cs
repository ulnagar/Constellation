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
            .ComplexProperty(item => item.AlertRecipient)
            .IsRequired();

        builder
            .ComplexProperty(item => item.AlertRecipient)
            .ComplexProperty(recipient => recipient.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(item => item.AlertRecipient)
            .ComplexProperty(recipient => recipient.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(item => item.AlertRecipient)
            .ComplexProperty(recipient => recipient.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .ComplexProperty(item => item.AlertRecipient)
            .Property(recipient => recipient.EmailAddress)
            .HasColumnName(nameof(EmailAddress))
            .IsRequired(false)
            .HasConversion(
                email => email.Email,
                email => EmailAddress.FromValue(email));

        builder
            .ComplexProperty(item => item.AlertRecipient)
            .Property(recipient => recipient.PhoneNumber)
            .HasColumnName(nameof(PhoneNumber))
            .IsRequired(false)
            .HasConversion<PhoneNumberConverter>();

    }
}
