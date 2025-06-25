namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Stocktake;

using Core.Models.Stocktake;
using Core.Models.Stocktake.Enums;
using Core.Models.Stocktake.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class DifferenceConfiguration : IEntityTypeConfiguration<Difference>
{
    public void Configure(EntityTypeBuilder<Difference> builder)
    {
        builder.ToTable("Differences", "Stocktake");

        builder
            .HasKey(difference => difference.Id);

        builder
            .Property(difference => difference.Id)
            .HasConversion(
                id => id.Value,
                value => DifferenceId.FromValue(value));

        builder
            .HasOne<StocktakeEvent>()
            .WithMany()
            .HasForeignKey(difference => difference.EventId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<StocktakeSighting>()
            .WithMany()
            .HasForeignKey(difference => difference.SightingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(difference => difference.Category)
            .HasConversion(
                category => category.Value,
                value => DifferenceCategory.FromValue(value));
    }
}