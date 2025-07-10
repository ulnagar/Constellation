namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Covers;

using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Covers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CoverConfiguration : IEntityTypeConfiguration<Cover>
{
    public void Configure(EntityTypeBuilder<Cover> builder)
    {
        builder.ToTable("Covers", "Covers");

        builder
            .HasKey(cover => cover.Id);

        builder
            .Property(cover => cover.Id)
            .HasConversion(
                coverId => coverId.Value,
                value => CoverId.FromValue(value));

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
                value => CoverTeacherType.FromValue(value));

        builder
            .HasDiscriminator<string>("CoverType")
            .HasValue<ClassCover>(nameof(ClassCover))
            .HasValue<AccessCover>(nameof(AccessCover));
    }
}

internal sealed class ClassCoverConfiguration : IEntityTypeConfiguration<ClassCover>
{
    public void Configure(EntityTypeBuilder<ClassCover> builder)
    {
        
    }
}

internal sealed class AccessCoverConfiguration : IEntityTypeConfiguration<AccessCover>
{
    public void Configure(EntityTypeBuilder<AccessCover> builder)
    {

    }
}