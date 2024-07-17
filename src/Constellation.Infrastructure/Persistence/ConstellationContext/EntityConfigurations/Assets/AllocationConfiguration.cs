namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assets;

using Core.Models.Assets;
using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AllocationConfiguration : IEntityTypeConfiguration<Allocation>
{
    public void Configure(EntityTypeBuilder<Allocation> builder)
    {
        builder.ToTable("Allocations", "Assets");

        builder
            .HasKey(allocation => allocation.Id);

        builder
            .Property(allocation => allocation.Id)
            .HasConversion(
                id => id.Value,
                value => AllocationId.FromValue(value));

        builder
            .Property(allocation => allocation.AllocationType)
            .HasConversion(
                entry => entry.Value,
                value => AllocationType.FromValue(value));
    }
}
