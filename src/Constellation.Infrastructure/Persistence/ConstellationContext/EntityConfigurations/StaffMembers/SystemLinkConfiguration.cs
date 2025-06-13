namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StaffMembers;

using Constellation.Core.Enums;
using Constellation.Core.Models.StaffMembers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StaffMemberSystemLinkConfiguration : IEntityTypeConfiguration<StaffMemberSystemLink>
{
    public void Configure(EntityTypeBuilder<StaffMemberSystemLink> builder)
    {
        builder.ToTable("SystemLinks", "Staff");

        builder
            .HasKey(systemLink => new { systemLink.StaffId, systemLink.System });

        builder
            .Property(systemLink => systemLink.System)
            .HasConversion(
                system => system.Value,
                value => SystemType.FromValue(value));
    }
}