namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserLoginAttemptConfiguration : IEntityTypeConfiguration<AppUserLoginAttempt>
{
    public void Configure(EntityTypeBuilder<AppUserLoginAttempt> builder)
    {
        builder.ToTable("AspNetUserLoginAttempts");

        builder
            .HasKey(attempt => new { attempt.AppUserId, attempt.LoginDateTime });

        builder
            .Property(attempt => attempt.Status)
            .HasConversion(
                status => status.Value,
                value => LoginStatus.FromValue(value)!);
    }
}