namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assets;

using Core.Models.Assets;
using Core.Models.Assets.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SightingConfiguration : IEntityTypeConfiguration<Sighting>
{
    public void Configure(EntityTypeBuilder<Sighting> builder)
    {
        builder.ToTable("Sightings", "Assets");

        builder
            .HasKey(sighting => sighting.Id);

        builder
            .Property(sighting => sighting.Id)
            .HasConversion(
                id => id.Value,
                value => SightingId.FromValue(value));

        builder
            .Property(sighting => sighting.Note)
            .IsRequired();
    }
}
