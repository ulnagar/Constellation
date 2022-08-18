using Constellation.Core.Models.Stocktake;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class StocktakeSightingConfiguration : IEntityTypeConfiguration<StocktakeSighting>
    {
        public void Configure(EntityTypeBuilder<StocktakeSighting> builder)
        {
            builder.HasKey(sighting => sighting.Id);
        }
    }
}
