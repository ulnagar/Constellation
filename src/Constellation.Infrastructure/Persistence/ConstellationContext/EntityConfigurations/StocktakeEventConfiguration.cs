namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models.Stocktake;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StocktakeEventConfiguration : IEntityTypeConfiguration<StocktakeEvent>
{
    public void Configure(EntityTypeBuilder<StocktakeEvent> builder)
    {
        builder.ToTable("StocktakeEvents");

        builder.HasKey(stocktake => stocktake.Id);

        builder.HasMany(stocktake => stocktake.Sightings).WithOne(sighting => sighting.StocktakeEvent).OnDelete(DeleteBehavior.Cascade);
    }
}