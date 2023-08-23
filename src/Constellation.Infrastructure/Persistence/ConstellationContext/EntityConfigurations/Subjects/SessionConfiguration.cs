namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Subjects;

using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Subjects_Sessions");

        builder
            .HasKey(session => session.Id);

        builder
            .HasOne(session => session.Offering)
            .WithMany(offering => offering.Sessions)
            .HasForeignKey(session => session.OfferingId);

        builder
            .Property(session => session.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .HasOne(session => session.Teacher)
            .WithMany(teacher => teacher.CourseSessions)
            .HasForeignKey(session => session.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(session => session.Room)
            .WithMany(room => room.OfferingSessions)
            .HasForeignKey(session => session.RoomId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(session => session.Period)
            .WithMany(period => period.OfferingSessions)
            .HasForeignKey(session => session.PeriodId);
    }
}