using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class ClassworkNotificationConfiguration : IEntityTypeConfiguration<ClassworkNotification>
    {
        public void Configure(EntityTypeBuilder<ClassworkNotification> builder)
        {
            builder.HasKey(notification => notification.Id);
            builder.HasOne(notification => notification.Offering).WithMany().HasForeignKey(notification => notification.OfferingId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(notification => notification.CompletedBy).WithMany().HasForeignKey(notification => notification.StaffId).OnDelete(DeleteBehavior.NoAction);
            builder.HasMany(notification => notification.Teachers).WithMany(teacher => teacher.ClassworkNotifications);
            builder.HasMany(notification => notification.Absences).WithMany(absence => absence.ClassworkNotifications);
        }
    }
}
