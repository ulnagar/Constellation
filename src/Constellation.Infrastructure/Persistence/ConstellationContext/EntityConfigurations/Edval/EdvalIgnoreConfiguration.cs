namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Core.Models.Edval.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EdvalIgnoreConfiguration : IEntityTypeConfiguration<EdvalIgnore>
{
    public void Configure(EntityTypeBuilder<EdvalIgnore> builder)
    {
        builder.ToTable("Ignore", "Edval");

        builder
            .HasKey(entity => new { entity.Type, entity.System, entity.Identifier });

        builder
            .Property(entity => entity.Type)
            .HasConversion(
                type => type.Value,
                value => EdvalDifferenceType.FromValue(value));

        builder
            .Property(entity => entity.System)
            .HasConversion(
                system => system.Value,
                value => EdvalDifferenceSystem.FromValue(value));
    }
}