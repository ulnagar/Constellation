using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.StudentId);

            builder.Property(s => s.Gender)
                .HasMaxLength(1);

            builder.HasOne(s => s.School)
                .WithMany(s => s.Students);

            builder.HasMany(s => s.AdobeConnectOperations);

            builder
                .HasMany(s => s.FamilyMemberships)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(student => student.Absences)
                .WithOne()
                .HasForeignKey(absence => absence.StudentId);

            builder
                .HasMany(student => student.AbsenceConfigurations)
                .WithOne()
                .HasForeignKey(config => config.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Navigation(student => student.AbsenceConfigurations)
                .AutoInclude();
        }
    }
}