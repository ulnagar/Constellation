namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Core.Models.Awards;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AcademicExcellenceScienceTechnologyNominationConfiguration
    : IEntityTypeConfiguration<AcademicExcellenceScienceTechnologyNomination>
{
    public void Configure(EntityTypeBuilder<AcademicExcellenceScienceTechnologyNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(AcademicExcellenceScienceTechnologyNomination.CourseId))
            .HasConversion(
                id => id.Value,
                value => CourseId.FromValue(value));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(AcademicExcellenceScienceTechnologyNomination.CourseName));

        builder
            .Property(nomination => nomination.OfferingId)
            .HasColumnName(nameof(AcademicExcellenceScienceTechnologyNomination.OfferingId))
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .Property(nomination => nomination.ClassName)
            .HasColumnName(nameof(AcademicExcellenceScienceTechnologyNomination.ClassName));
    }
}