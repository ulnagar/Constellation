using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
    {
        public void Configure(EntityTypeBuilder<Absence> builder)
        {
            builder.HasKey(absence => absence.Id);

            builder.Property(absence => absence.Id).ValueGeneratedOnAdd();

            builder.HasOne(absence => absence.Student)
                .WithMany(student => student.Absences)
                .HasForeignKey(absence => absence.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(absence => absence.Offering)
                .WithMany(course => course.Absences)
                .HasForeignKey(absence => absence.OfferingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
