namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Absences;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Absences_Notifications");

        builder
            .HasKey(notification => notification.Id);

        builder
            .Property(notification => notification.Id)
            .HasConversion(
                id => id.Value,
                value => AbsenceNotificationId.FromValue(value));

        builder
            .Property(notification => notification.Type)
            .HasConversion(
                entry => entry.Value,
                value => NotificationType.FromValue(value));
    }
}
