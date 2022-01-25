using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class AbsenceNotificationConfiguration : IEntityTypeConfiguration<AbsenceNotification>
    {
        public void Configure(EntityTypeBuilder<AbsenceNotification> builder)
        {
            builder.HasKey(notification => notification.Id);

            builder.Property(notification => notification.Id).ValueGeneratedOnAdd();

            builder.HasOne(notification => notification.Absence)
                .WithMany(absence => absence.Notifications)
                .HasForeignKey(notification => notification.AbsenceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
