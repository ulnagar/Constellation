namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Application.Models.Identity;
using Core.Models.SchoolContacts.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder
            .Property(user => user.SchoolContactId)
            .HasConversion(
                id => id.Value,
                value => SchoolContactId.FromValue(value));
    }
}
