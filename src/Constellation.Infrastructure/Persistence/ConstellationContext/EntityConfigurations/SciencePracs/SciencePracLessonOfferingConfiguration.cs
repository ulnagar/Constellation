namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SciencePracs;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SciencePracLessonOfferingConfiguration : IEntityTypeConfiguration<SciencePracLessonOffering>
{
    public void Configure(EntityTypeBuilder<SciencePracLessonOffering> builder)
    {
        builder.ToTable("LessonOfferings", "SciencePracs");

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
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(entity => entity.OfferingId);

        builder
            .Property(entity => entity.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));
    }
}