namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class FirstInSubjectNominationConfiguration :
    IEntityTypeConfiguration<FirstInSubjectNomination>
{
    public void Configure(EntityTypeBuilder<FirstInSubjectNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(FirstInSubjectNomination.CourseId));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(FirstInSubjectNomination.CourseName));
    }
}
