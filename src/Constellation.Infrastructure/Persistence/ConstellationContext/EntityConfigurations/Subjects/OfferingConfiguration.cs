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
            .HasOne(offering => offering.Course)
            .WithMany(course => course.Offerings)
            .HasForeignKey(offering => offering.CourseId);

        builder
            .Navigation(offering => offering.Course)
            .AutoInclude();

        builder
            .HasMany(offering => offering.Enrolments)
            .WithOne(enrolment => enrolment.Offering);

        builder
            .HasMany(offering => offering.Sessions)
            .WithOne(session => session.Offering);

        builder
            .Navigation(offering => offering.Sessions)
            .AutoInclude();

        builder
            .HasMany(offering => offering.Absences)
            .WithOne()
            .HasForeignKey(absence => absence.OfferingId);

        builder
            .HasMany(offering => offering.Resources)
            .WithOne(resource => resource.Offering)
            .HasForeignKey(resource => resource.OfferingId);

        builder
            .Navigation(offering => offering.Resources)
            .AutoInclude();
    }
}
