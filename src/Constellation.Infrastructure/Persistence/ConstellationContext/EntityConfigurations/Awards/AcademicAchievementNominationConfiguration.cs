namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AcademicAchievementNominationConfiguration
    : IEntityTypeConfiguration<AcademicAchievementNomination>
{
    public void Configure(EntityTypeBuilder<AcademicAchievementNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(AcademicAchievementNomination.CourseId));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(AcademicAchievementNomination.CourseName));

        builder
            .Property(nomination => nomination.OfferingId)
            .HasColumnName(nameof(AcademicAchievementNomination.OfferingId));

        builder
            .Property(nomination => nomination.ClassName)
            .HasColumnName(nameof(AcademicAchievementNomination.ClassName));
    }
}