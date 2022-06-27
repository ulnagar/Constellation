using Constellation.Core.Models.Stocktake;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class StocktakeEventConfiguration : IEntityTypeConfiguration<StocktakeEvent>
    {
        public void Configure(EntityTypeBuilder<StocktakeEvent> builder)
        {
            builder.HasKey(stocktake => stocktake.Id);

            builder.HasMany(stocktake => stocktake.Sightings).WithOne(sighting => sighting.StocktakeEvent).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
