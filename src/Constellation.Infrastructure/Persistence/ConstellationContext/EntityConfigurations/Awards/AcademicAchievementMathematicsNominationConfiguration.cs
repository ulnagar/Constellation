namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Core.Models.Awards;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AcademicAchievementMathematicsNominationConfiguration
    : IEntityTypeConfiguration<AcademicAchievementMathematicsNomination>
{
    public void Configure(EntityTypeBuilder<AcademicAchievementMathematicsNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(AcademicAchievementMathematicsNomination.CourseId))
            .HasConversion(
                id => id.Value,
                value => CourseId.FromValue(value));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(AcademicAchievementMathematicsNomination.CourseName));

        builder
            .Property(nomination => nomination.OfferingId)
            .HasColumnName(nameof(AcademicAchievementMathematicsNomination.OfferingId))
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .Property(nomination => nomination.ClassName)
            .HasColumnName(nameof(AcademicAchievementMathematicsNomination.ClassName));
    }
}