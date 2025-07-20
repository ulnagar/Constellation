namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Tutorials;

using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialSessionConfiguration : IEntityTypeConfiguration<TutorialSession>
{
    public void Configure(EntityTypeBuilder<TutorialSession> builder)
    {
        builder.ToTable("Sessions", "Tutorials");

        builder
            .HasKey(session => session.Id);

        builder
            .Property(session => session.Id)
            .HasConversion(
                id => id.Value,
                value => TutorialSessionId.FromValue(value));

        builder
            .Property(session => session.Week)
            .HasConversion(
                week => week.Value,
                value => PeriodWeek.FromValue(value));

        builder
            .Property(session => session.Day)
            .HasConversion(
                day => day.Value,
                value => PeriodDay.FromValue(value));

        builder
            .HasOne<StaffMember>()
            .WithMany()
            .HasForeignKey(session => session.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(session => session.StaffId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));
    }
}