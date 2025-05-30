﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class DifferenceConfiguration : IEntityTypeConfiguration<Difference>
{
    public void Configure(EntityTypeBuilder<Difference> builder)
    {
        builder.ToTable("Differences", "Edval");

        builder
            .HasKey(entity => entity.Id);

        builder
            .Property(entity => entity.Id)
            .HasConversion(
                id => id.Value,
                value => DifferenceId.FromValue(value));

        builder
            .Property(entity => entity.Type)
            .HasConversion(
                type => type.Value,
                value => EdvalDifferenceType.FromValue(value));

        builder
            .Property(entity => entity.System)
            .HasConversion(
                type => type.Value,
                value => EdvalDifferenceSystem.FromValue(value));
    }
}