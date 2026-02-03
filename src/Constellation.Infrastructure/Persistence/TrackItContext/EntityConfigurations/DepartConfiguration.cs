using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class DepartConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> entity)
        {
            entity.HasKey(e => e.Sequence).HasName("PK_DEPART_");

            entity.ToTable("_DEPART_", "_SMDBA_", tb =>
            {
                tb.HasTrigger("sm_ad__DEPART_st");
                tb.HasTrigger("sm_ai__DEPART_st");
                tb.HasTrigger("sm_au__DEPART_st");
            });

            entity.HasIndex(e => new { e.Assistmanager, e.Sequence }, "FK_DEPART_ASSISTMANAGER");

            entity.HasIndex(e => new { e.Location, e.Sequence }, "FK_DEPART_LOCATION");

            entity.HasIndex(e => new { e.SeqDeptmanager, e.Sequence }, "FK_DEPART_SEQ_DEPTMANAGER");

            entity.HasIndex(e => e.SeqPriority, "FK_DEPART_SEQ_PRIORITY");

            entity.HasIndex(e => new { e.Group, e.Sequence }, "FK_DEPART__GROUP_");

            entity.HasIndex(e => e.Name, "UQ_DEPART_NAME").IsUnique();

            entity.Property(e => e.Sequence)
                .ValueGeneratedNever()
                .HasColumnName("SEQUENCE");
            entity.Property(e => e.Assistmanager).HasColumnName("ASSISTMANAGER");
            entity.Property(e => e.Dept)
                .HasMaxLength(15)
                .HasColumnName("DEPT");
            entity.Property(e => e.Fax)
                .HasMaxLength(30)
                .HasColumnName("FAX");
            entity.Property(e => e.Group).HasColumnName("_GROUP_");
            entity.Property(e => e.Inactive).HasColumnName("_INACTIVE_:");
            entity.Property(e => e.Lastmodified)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LASTMODIFIED");
            entity.Property(e => e.Lastuser)
                .HasMaxLength(255)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("LASTUSER");
            entity.Property(e => e.Location).HasColumnName("LOCATION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.Note).HasColumnName("NOTE");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("PHONE");
            entity.Property(e => e.SeqDeptmanager).HasColumnName("SEQ_DEPTMANAGER");
            entity.Property(e => e.SeqPriority).HasColumnName("SEQ_PRIORITY");
        }
    }

}
