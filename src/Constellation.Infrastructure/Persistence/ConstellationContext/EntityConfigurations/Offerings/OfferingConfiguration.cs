namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Offerings;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OfferingConfiguration : IEntityTypeConfiguration<Offering>
{
    public void Configure(EntityTypeBuilder<Offering> builder)
    {
        builder.ToTable("Offerings_Offerings");

        builder
            .HasKey(offering => offering.Id);

        builder
            .Property(offering => offering.Id)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .Property(offering => offering.StartDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(offering => offering.EndDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .HasOne<Course>()
            .WithMany(course => course.Offerings)
            .HasForeignKey(offering => offering.CourseId);

        builder
            .HasMany<Enrolment>()
            .WithOne();

        builder
            .HasMany(offering => offering.Sessions)
            .WithOne(session => session.Offering);

        builder
            .Navigation(offering => offering.Sessions)
            .AutoInclude();

        builder
            .HasMany<Absence>()
            .WithOne()
            .HasForeignKey(absence => absence.OfferingId);

        builder
            .HasMany(offering => offering.Resources)
            .WithOne(resource => resource.Offering)
            .HasForeignKey(resource => resource.OfferingId);

        builder
            .Navigation(offering => offering.Resources)
            .AutoInclude();

        builder
            .HasMany(offering => offering.Teachers)
            .WithOne(assignment => assignment.Offering)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
