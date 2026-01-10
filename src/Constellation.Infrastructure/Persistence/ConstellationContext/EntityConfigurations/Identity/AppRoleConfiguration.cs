namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder
            .Property(role => role.Type)
            .HasConversion(
                status => status.Value,
                value => AppRoleType.FromValue(value)!);
    }
}