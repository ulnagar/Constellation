﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SciencePracs;

using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SciencePracLessonOfferingConfiguration : IEntityTypeConfiguration<SciencePracLessonOffering>
{
    public void Configure(EntityTypeBuilder<SciencePracLessonOffering> builder)
    {
        builder.ToTable("SciencePracs_Lessons_Offerings");

        builder
            .HasKey(entity => new { entity.LessonId, entity.OfferingId });

        builder
            .HasOne<SciencePracLesson>()
            .WithMany(lesson => lesson.Offerings)
            .HasForeignKey(entity => entity.LessonId);

        builder
            .Property(entity => entity.LessonId)
            .HasConversion(
                id => id.Value,
                value => SciencePracLessonId.FromValue(value));

        builder
            .HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(entity => entity.OfferingId);
    }
}