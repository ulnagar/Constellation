namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assets;

using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets", "Assets");

        builder
            .HasKey(asset => asset.Id);

        builder
            .Property(asset => asset.Id)
            .HasConversion(
                id => id.Value,
                value => AssetId.FromValue(value));

        builder
            .Property(asset => asset.AssetNumber)
            .HasConversion(
                number => number.ToString(),
                value => AssetNumber.FromValue(value));

        builder
            .Property(asset => asset.Status)
            .HasConversion(
                status => status.Value,
                value => AssetStatus.FromValue(value));

        builder
            .Property(asset => asset.Category)
            .HasConversion(
                category => category.Value,
                value => AssetCategory.FromValue(value));

        builder
            .HasMany(asset => asset.Allocations)
            .WithOne()
            .HasForeignKey(allocation => allocation.AssetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(asset => asset.Allocations)
            .AutoInclude();

        builder
            .HasMany(asset => asset.Locations)
            .WithOne()
            .HasForeignKey(location => location.AssetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(asset => asset.Locations)
            .AutoInclude();

        builder
            .HasMany(asset => asset.Sightings)
            .WithOne()
            .HasForeignKey(sighting => sighting.AssetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(asset => asset.Sightings)
            .AutoInclude();

        builder
            .HasMany(asset => asset.Notes)
            .WithOne()
            .HasForeignKey(note => note.AssetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(asset => asset.Notes)
            .AutoInclude();

        builder
            .HasIndex(asset => asset.AssetNumber)
            .IsUnique();

        builder
            .HasIndex(asset => asset.SerialNumber)
            .IsUnique();

        builder
            .HasIndex(asset => asset.SapEquipmentNumber)
            .IsUnique();
    }
}
