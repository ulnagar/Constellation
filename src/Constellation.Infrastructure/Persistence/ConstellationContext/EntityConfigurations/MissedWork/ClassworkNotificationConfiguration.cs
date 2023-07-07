namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.MissedWork;

using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MissedWork;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ClassworkNotificationConfiguration : IEntityTypeConfiguration<ClassworkNotification>
{
    public void Configure(EntityTypeBuilder<ClassworkNotification> builder)
    {
        builder.ToTable("MissedWork_ClassworkNotifications");

        builder
            .HasKey(notification => notification.Id);

        builder
            .Property(notification => notification.Id)
            .HasConversion(
                id => id.Value,
                value => ClassworkNotificationId.FromValue(value));

        builder
            .HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(notification => notification.OfferingId);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(notification => notification.StaffId);

        builder
            .HasMany<Absence>(notification => notification.Absences)
            .WithMany();

        builder
            .HasMany<Staff>(notification => notification.Teachers)
            .WithMany();
            //.UsingEntity("_Staff_ClassworkNotification");

        builder
            .Property(notification => notification.AbsenceDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}
