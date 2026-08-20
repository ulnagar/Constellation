namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Core.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserPasskeyConfiguration : IEntityTypeConfiguration<AppUserPasskey>
{
    public void Configure(EntityTypeBuilder<AppUserPasskey> builder)
    {
        builder.ToTable("AspNetUserPasskeys");

        builder
            .HasKey(entry => entry.CredentialId);

        builder
            .Property(entry => entry.CredentialId)
            .ValueGeneratedNever();

        builder
            .HasOne(entry => entry.User)
            .WithMany(entry => entry.PasskeyCredentials)
            .HasForeignKey(entry => entry.AppUserId);
    }
}