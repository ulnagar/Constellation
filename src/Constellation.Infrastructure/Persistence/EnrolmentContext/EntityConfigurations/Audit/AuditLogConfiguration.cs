namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.Audit;

using Constellation.Core.Models.EnrolmentContext.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .UseIdentityColumn();

        builder
            .Property(x => x.EntityName)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(x => x.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(x => x.PropertyName)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(x => x.OldValue)
            .HasColumnType("nvarchar(max)");

        builder
            .Property(x => x.NewValue)
            .HasColumnType("nvarchar(max)");

        builder
            .Property(x => x.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder
            .Property(x => x.ChangedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder
            .Property(x => x.Timestamp)
            .IsRequired();

        builder
            .Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder
            .HasIndex(x => new { x.EntityName, x.EntityId })
            .HasDatabaseName("IX_AuditLogs_EntityName_EntityId");

        builder
            .HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder
            .HasIndex(x => x.ChangedBy)
            .HasDatabaseName("IX_AuditLogs_ChangedBy");
    }
}
