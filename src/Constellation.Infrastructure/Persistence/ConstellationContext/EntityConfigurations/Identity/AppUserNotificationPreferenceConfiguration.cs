namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserNotificationPreferenceConfiguration : IEntityTypeConfiguration<AppUserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<AppUserNotificationPreference> builder)
    {
        builder.ToTable("AspNetUserNotificationPreferences");

        builder
            .HasKey(entry => new { entry.AppUserId, entry.NotificationType });

        builder
            .Property(entry => entry.NotificationType)
            .HasConversion(
                type => type.Value,
                value => NotificationType.FromValue(value));
    }
}