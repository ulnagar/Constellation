namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models.Stocktake;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StocktakeSightingConfiguration : IEntityTypeConfiguration<StocktakeSighting>
{
    public void Configure(EntityTypeBuilder<StocktakeSighting> builder)
    {
        builder.ToTable("StocktakeSightings");

        builder.HasKey(sighting => sighting.Id);
    }
}