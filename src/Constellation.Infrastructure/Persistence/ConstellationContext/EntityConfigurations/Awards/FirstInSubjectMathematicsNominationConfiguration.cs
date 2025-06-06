﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Core.Models.Awards;
using Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class FirstInSubjectMathematicsNominationConfiguration :
    IEntityTypeConfiguration<FirstInSubjectMathematicsNomination>
{
    public void Configure(EntityTypeBuilder<FirstInSubjectMathematicsNomination> builder)
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