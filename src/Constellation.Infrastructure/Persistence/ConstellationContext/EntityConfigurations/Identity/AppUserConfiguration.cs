namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Application.Models.Identity;
using Core.Models.Auth;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder
            .ComplexProperty(user => user.Name)
            .IsRequired();

        builder
            .ComplexProperty(user => user.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(user => user.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(user => user.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();
        
        builder
            .HasMany(user => user.Logins)
            .WithOne()
            .HasForeignKey(user => user.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(user => user.Logins)
            .AutoInclude();

        builder
            .HasMany(user => user.Links)
            .WithOne()
            .HasForeignKey(link => link.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(user => user.Links)
            .AutoInclude();

        builder
            .Navigation(user => user.PasskeyCredentials)
            .AutoInclude();
    }
}