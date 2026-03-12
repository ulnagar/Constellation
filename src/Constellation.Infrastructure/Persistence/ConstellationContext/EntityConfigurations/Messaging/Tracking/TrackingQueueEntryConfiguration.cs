namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Tracking;

using Core.Models.Messaging.Tracking;
using Core.Models.Messaging.Tracking.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

internal sealed class TrackingQueueEntryConfiguration : IEntityTypeConfiguration<TrackingQueueEntry>
{
    public void Configure(EntityTypeBuilder<TrackingQueueEntry> builder)
    {
        builder.ToTable("TrackingEventQueue", "Automation");

        builder
            .HasKey(entry => entry.Id);

        builder
            .Property(entry => entry.Id)
            .HasConversion(
                id => id.Value,
                value => TrackingQueueEntryId.FromValue(value));

        builder
            .Property(entry => entry.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder
            .Property(entry => entry.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder
            .Property(entry => entry.EnqueuedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder
            .Property(entry => entry.LastError)
            .HasMaxLength(2000);

        builder
            .HasIndex(entry => new { entry.RetryAfter, entry.EnqueuedAt })
            .HasDatabaseName("IX_Automation_TrackingEventQueue_RetryAfter_EnqueuedAt");
    }
}
