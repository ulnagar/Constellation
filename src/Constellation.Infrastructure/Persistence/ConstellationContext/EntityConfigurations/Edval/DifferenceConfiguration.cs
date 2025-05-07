namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Core.Models.Edval.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class DifferenceConfiguration : IEntityTypeConfiguration<Difference>
{
    public void Configure(EntityTypeBuilder<Difference> builder)
    {
        builder.ToTable("Differences", "Edval");

        builder
            .Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasAnnotation("Key", 0);

        builder
            .Property(entity => entity.Type)
            .HasConversion(
                type => type.Value,
                value => EdvalDifferenceType.FromValue(value));
    }
}