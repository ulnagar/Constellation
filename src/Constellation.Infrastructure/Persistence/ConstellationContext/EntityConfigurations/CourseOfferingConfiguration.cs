using Constellation.Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class CourseOfferingConfiguration : IEntityTypeConfiguration<Offering>
    {
        public void Configure(EntityTypeBuilder<Offering> builder)
        {
            builder.HasKey(o => o.Id);

            builder.HasOne(o => o.Course)
                .WithMany(c => c.Offerings)
                .HasForeignKey(o => o.CourseId);

            builder.HasMany(o => o.Enrolments)
                .WithOne(e => e.Offering);

            builder.HasMany(o => o.Sessions)
                .WithOne(s => s.Offering);

            builder
                .HasMany(offering => offering.Absences)
                .WithOne()
                .HasForeignKey(absence => absence.OfferingId);
        }
    }
}