namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Stocktake;

using Constellation.Core.Models.Stocktake;
using Core.Models.Stocktake.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StocktakeEventConfiguration : IEntityTypeConfiguration<StocktakeEvent>
{
    public void Configure(EntityTypeBuilder<StocktakeEvent> builder)
    {
        builder.ToTable("Events", "Stocktake");

        builder
            .HasKey(@event => @event.Id);

        builder
            .Property(@event => @event.Id)
            .HasConversion(
                id => id.Value,
                value => StocktakeEventId.FromValue(value));

        builder
            .HasMany(stocktake => stocktake.Sightings)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(stocktake => stocktake.Sightings)
            .AutoInclude();

        builder
            .HasMany(stocktake => stocktake.Differences)
            .WithOne()
            .HasForeignKey(difference => difference.EventId);

        builder
            .Navigation(stocktake => stocktake.Differences)
            .AutoInclude();
    }
}