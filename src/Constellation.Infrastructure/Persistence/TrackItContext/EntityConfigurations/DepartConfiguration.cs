using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class DepartConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(e => e.Sequence).HasName("PK_DEPART_");
            builder.ToTable("_DEPART_", "_SMDBA_");
            builder.HasIndex(e => new { e.Assistmanager, e.Sequence }, "FK_DEPART_ASSISTMANAGER");
            builder.HasIndex(e => new { e.Location, e.Sequence }, "FK_DEPART_LOCATION");
            builder.HasIndex(e => new { e.SeqDeptmanager, e.Sequence }, "FK_DEPART_SEQ_DEPTMANAGER");
            builder.HasIndex(e => e.SeqPriority, "FK_DEPART_SEQ_PRIORITY");
            builder.HasIndex(e => new { e.Group, e.Sequence }, "FK_DEPART__GROUP_");
            builder.HasIndex(e => e.Name, "UQ_DEPART_NAME").IsUnique();
            builder.Property(e => e.Sequence).ValueGeneratedNever().HasColumnName("SEQUENCE");
            builder.Property(e => e.Assistmanager).HasColumnName("ASSISTMANAGER");
            builder.Property(e => e.Dept).HasMaxLength(15).HasColumnName("DEPT");
            builder.Property(e => e.Fax).HasMaxLength(30).HasColumnName("FAX");
            builder.Property(e => e.Group).HasColumnName("_GROUP_");
            builder.Property(e => e.Inactive).HasColumnName("_INACTIVE_:");
            builder.Property(e => e.Lastmodified).HasColumnType("datetime").HasColumnName("LASTMODIFIED").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.Lastuser).HasMaxLength(255).HasColumnName("LASTUSER").HasDefaultValueSql("(user_name())");
            builder.Property(e => e.Location).HasColumnName("LOCATION");
            builder.Property(e => e.Name).IsRequired().HasMaxLength(50).HasColumnName("NAME");
            builder.Property(e => e.Note).HasColumnName("NOTE");
            builder.Property(e => e.Phone).HasMaxLength(30).HasColumnName("PHONE");
            builder.Property(e => e.SeqDeptmanager).HasColumnName("SEQ_DEPTMANAGER");
            builder.Property(e => e.SeqPriority).HasColumnName("SEQ_PRIORITY");
        }
    }

}
