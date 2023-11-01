namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class FirstInSubjectNominationConfiguration :
    IEntityTypeConfiguration<FirstInSubjectNomination>
{
    public void Configure(EntityTypeBuilder<FirstInSubjectNomination> builder)
    {
        builder
            .Property(nomination => nomination.CourseId)
            .HasColumnName(nameof(FirstInSubjectNomination.CourseId))
            .HasConversion(
                id => id.Value,
                value => CourseId.FromValue(value));

        builder
            .Property(nomination => nomination.CourseName)
            .HasColumnName(nameof(FirstInSubjectNomination.CourseName));

        builder
            .Property(nomination => nomination.Grade)
            .HasColumnName(nameof(FirstInSubjectNomination.Grade));
    }
}
