namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SciencePracs;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SciencePracLessonConfiguration : IEntityTypeConfiguration<SciencePracLesson>
{
    public void Configure(EntityTypeBuilder<SciencePracLesson> builder)
    {
        builder.ToTable("SciencePracs_Lessons");

        builder
            .HasKey(lesson => lesson.Id);

        builder
            .Property(lesson => lesson.Id)
            .HasConversion(
                id => id.Value,
                value => SciencePracLessonId.FromValue(value));

        builder
            .HasMany(lesson => lesson.Offerings)
            .WithOne();

        builder
            .HasMany(lesson => lesson.Rolls)
            .WithOne()
            .HasForeignKey(roll => roll.LessonId);

        builder
            .Navigation(lesson => lesson.Offerings)
            .AutoInclude();

        builder
            .Navigation(lesson => lesson.Rolls)
            .AutoInclude();
    }
}
