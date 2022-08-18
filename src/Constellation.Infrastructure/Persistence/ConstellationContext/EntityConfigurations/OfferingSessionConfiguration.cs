using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class OfferingSessionConfiguration : IEntityTypeConfiguration<OfferingSession>
    {
        public void Configure(EntityTypeBuilder<OfferingSession> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Offering)
                .WithMany(o => o.Sessions)
                .HasForeignKey(s => s.OfferingId);

            builder.HasOne(s => s.Teacher)
                .WithMany(t => t.CourseSessions)
                .HasForeignKey(s => s.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(s => s.Room)
                .WithMany(r => r.OfferingSessions)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(s => s.Period)
                .WithMany(p => p.OfferingSessions)
                .HasForeignKey(s => s.PeriodId);
        }
    }
}