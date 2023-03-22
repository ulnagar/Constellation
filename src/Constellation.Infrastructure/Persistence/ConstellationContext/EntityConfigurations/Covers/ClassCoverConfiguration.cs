namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Covers;

using Constellation.Core.Models;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
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
            .HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(cover => cover.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(cover => cover.TeacherType)
            .HasConversion(
                teacherType => teacherType.Value, 
                value => CoverTeacherType.ByValue(value));

        builder
            .Property(cover => cover.StartDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(cover => cover.EndDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}