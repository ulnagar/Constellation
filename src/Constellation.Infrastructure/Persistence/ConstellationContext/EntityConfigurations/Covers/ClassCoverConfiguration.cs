namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Covers;

using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ClassCoverConfiguration : IEntityTypeConfiguration<ClassCover>
{
    public void Configure(EntityTypeBuilder<ClassCover> builder)
    {
        builder.ToTable("Covers_ClassCovers");

        builder
            .HasKey(cover => cover.Id);

        builder
            .Property(cover => cover.Id)
            .HasConversion(
                coverId => coverId.Value,
                value => ClassCoverId.FromValue(value));

        builder
            .Property(cover => cover.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(cover => cover.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(cover => cover.TeacherType)
            .HasConversion(
                teacherType => teacherType.Value, 
                value => CoverTeacherType.ByValue(value));
    }
}