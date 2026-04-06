namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Email;

using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EmailTrackingEventConfiguration : IEntityTypeConfiguration<EmailTrackingEvent>
{
    public void Configure(EntityTypeBuilder<EmailTrackingEvent> builder)
    {
        builder.ToTable("EmailTrackingEvents", "Messages");

        builder
            .HasKey(e => e.Id);

        builder
            .Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => EmailTrackingEventId.FromValue(value));

        builder
            .Property(e => e.OccurredAt)
            .IsRequired();

        builder
            .Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(e => e.IpAddress)
            .HasMaxLength(45);    // covers IPv6

        builder
            .Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder
            .Property(e => e.LinkUrl)
            .HasMaxLength(2000);

        builder
            .Property(e => e.Metadata)
            .HasColumnType("nvarchar(max)");

        // Supports queries like "get all open events for this message"
        builder
            .HasIndex(e => new { e.EmailId, e.EventType });

        // Supports queries like "get all events in a time range"
        builder
            .HasIndex(e => e.OccurredAt);
    }
}