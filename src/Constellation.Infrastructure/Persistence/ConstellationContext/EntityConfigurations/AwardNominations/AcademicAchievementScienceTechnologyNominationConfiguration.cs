namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AwardNominations;

using Core.Models.Awards;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AcademicAchievementScienceTechnologyNominationConfiguration
    : IEntityTypeConfiguration<AcademicAchievementScienceTechnologyNomination>
{
    public void Configure(EntityTypeBuilder<AcademicAchievementScienceTechnologyNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(AcademicAchievementScienceTechnologyNomination.CourseId))
            .HasConversion(
                id => id.Value,
                value => CourseId.FromValue(value));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(AcademicAchievementScienceTechnologyNomination.CourseName));

        builder
            .Property(nomination => nomination.OfferingId)
            .HasColumnName(nameof(AcademicAchievementScienceTechnologyNomination.OfferingId))
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .Property(nomination => nomination.ClassName)
            .HasColumnName(nameof(AcademicAchievementScienceTechnologyNomination.ClassName));
    }
}