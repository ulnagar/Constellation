﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assets;

using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations", "Assets");

        builder
            .HasKey(location => location.Id);

        builder
            .Property(location => location.Id)
            .HasConversion(
                id => id.Value,
                value => LocationId.FromValue(value));

        builder
            .Property(location => location.Category)
            .HasConversion(
                category => category.Value,
                value => LocationCategory.FromValue(value));

        builder
            .HasIndex(location => location.SchoolCode);
    }
}
