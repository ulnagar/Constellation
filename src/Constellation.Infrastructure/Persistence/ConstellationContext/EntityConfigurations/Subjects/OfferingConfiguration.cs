namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Subjects;

using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OfferingConfiguration : IEntityTypeConfiguration<Offering>
{
    public void Configure(EntityTypeBuilder<Offering> builder)
    {
        builder.ToTable("Subjects_Offerings");

        builder
            .HasKey(offering => offering.Id);

        builder
            .Property(offering => offering.Id)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .HasOne(o => o.Course)
            .WithMany(c => c.Offerings)
            .HasForeignKey(o => o.CourseId);

        builder
            .HasMany(o => o.Enrolments)
            .WithOne(e => e.Offering);

        builder
            .HasMany(o => o.Sessions)
            .WithOne(s => s.Offering);

        builder
            .HasMany(offering => offering.Absences)
            .WithOne()
            .HasForeignKey(absence => absence.OfferingId);
    }
}
