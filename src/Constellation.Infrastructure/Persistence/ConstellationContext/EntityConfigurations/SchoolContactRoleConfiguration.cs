using Constellation.Core.Models.SchoolContacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class SchoolContactRoleConfiguration : IEntityTypeConfiguration<SchoolContactRole>
    {
        public void Configure(EntityTypeBuilder<SchoolContactRole> builder)
        {
            builder.ToTable("SchoolContactRole");

            builder.HasKey(s => s.Id);

            builder.HasOne(a => a.SchoolContact)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.SchoolContactId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.School)
                .WithMany(s => s.StaffAssignments)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}