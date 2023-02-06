namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Covers;

using Constellation.Core.Models.Covers;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClassCoverConfiguration : IEntityTypeConfiguration<ClassCover>
{
    public void Configure(EntityTypeBuilder<ClassCover> builder)
    {
        builder.ToTable("Covers_ClassCovers");

        builder
            .HasKey(cover => cover.Id);

        builder
            .Property(cover => cover.StartDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(cover => cover.EndDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}