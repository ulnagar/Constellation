namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserLinkConfiguration : IEntityTypeConfiguration<AppUserLink>
{
    public void Configure(EntityTypeBuilder<AppUserLink> builder)
    {
        builder.ToTable("AspNetUserLinks");

        builder
            .HasKey(link => link.Id);

        builder
            .Property(link => link.Type)
            .HasConversion(
                type => type.Value,
                value => LinkType.FromValue(value)!);
    }
}