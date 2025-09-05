namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AwardNominations;

using Converters;
using Core.Models.Awards;
using Core.Models.Awards.Enums;
using Core.Models.Awards.Identifiers;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NominationNotificationConfiguration : IEntityTypeConfiguration<NominationNotification>
{
    public void Configure(EntityTypeBuilder<NominationNotification> builder)
    {
        builder.ToTable("Notifications", "AwardNominations");

        builder
            .HasKey(notification => notification.Id);

        builder
            .Property(notification => notification.Id)
            .HasConversion(
                id => id.Value,
                value => NominationNotificationId.FromValue(value));

        builder
            .Property(notification => notification.Type)
            .HasConversion(
                type => type.Value,
                value => AwardNotificationType.FromValue(value));

        builder
            .Property(notification => notification.FromAddress)
            .HasConversion(new JsonColumnConverter<EmailRecipient>());

        builder
            .Property(notification => notification.ToAddresses)
            .HasConversion(new JsonColumnConverter<IReadOnlyList<EmailRecipient>>());

        builder
            .Property(notification => notification.CcAddresses)
            .HasConversion(new JsonColumnConverter<IReadOnlyList<EmailRecipient>>());

        builder
            .Property(notification => notification.Nominations)
            .HasConversion(new JsonColumnConverter<IReadOnlyList<AwardNominationId>>());

        builder
            .HasOne<NominationPeriod>()
            .WithMany()
            .HasForeignKey(notification => notification.PeriodId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}