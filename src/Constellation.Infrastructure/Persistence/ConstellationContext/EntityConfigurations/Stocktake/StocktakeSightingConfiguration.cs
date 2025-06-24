namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Stocktake;

using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Stocktake;
using Core.Models.Stocktake.Enums;
using Core.Models.Stocktake.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StocktakeSightingConfiguration : IEntityTypeConfiguration<StocktakeSighting>
{
    public void Configure(EntityTypeBuilder<StocktakeSighting> builder)
    {
        builder.ToTable("Sightings", "Stocktake");

        builder
            .HasKey(sighting => sighting.Id);

        builder
            .Property(sighting => sighting.Id)
            .HasConversion(
                id => id.Value,
                value => StocktakeSightingId.FromValue(value));

        builder
            .Property(sighting => sighting.StocktakeEventId)
            .HasConversion(
                id => id.Value,
                value => StocktakeEventId.FromValue(value));

        builder
            .Property(sighting => sighting.AssetNumber)
            .HasConversion(
                number => number.ToString(),
                value => AssetNumber.FromValue(value));

        builder
            .Property(sighting => sighting.LocationCategory)
            .HasConversion(
                category => category.Value,
                value => LocationCategory.FromValue(value));

        builder
            .Property(sighting => sighting.UserType)
            .HasConversion(
                user => user.Value,
                value => UserType.FromValue(value));
    }
}