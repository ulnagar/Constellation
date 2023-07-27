namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AcademicExcellenceNominationConfiguration
    : IEntityTypeConfiguration<AcademicExcellenceNomination>
{
    public void Configure(EntityTypeBuilder<AcademicExcellenceNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(AcademicExcellenceNomination.CourseId));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(AcademicExcellenceNomination.CourseName));

        builder
            .Property(nomination => nomination.OfferingId)
            .HasColumnName(nameof(AcademicExcellenceNomination.OfferingId));

        builder
            .Property(nomination => nomination.ClassName)
            .HasColumnName(nameof(AcademicExcellenceNomination.ClassName));
    }
}
