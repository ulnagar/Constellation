namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AwardNominations;

using Core.Models.Awards;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AcademicExcellenceMathematicsNominationConfiguration
    : IEntityTypeConfiguration<AcademicExcellenceMathematicsNomination>
{
    public void Configure(EntityTypeBuilder<AcademicExcellenceMathematicsNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(AcademicExcellenceMathematicsNomination.CourseId))
            .HasConversion(
                id => id.Value,
                value => CourseId.FromValue(value));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(AcademicExcellenceMathematicsNomination.CourseName));

        builder
            .Property(nomination => nomination.OfferingId)
            .HasColumnName(nameof(AcademicExcellenceMathematicsNomination.OfferingId))
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .Property(nomination => nomination.ClassName)
            .HasColumnName(nameof(AcademicExcellenceMathematicsNomination.ClassName));
    }
}