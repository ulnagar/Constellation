namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attendance;

using Core.Models.Attendance;
using Core.Models.Attendance.Identifiers;
using Core.Models.Offerings;
using Core.Models.Subjects;
using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AttendancePlanPeriodConfiguration : IEntityTypeConfiguration<AttendancePlanPeriod>
{
    public void Configure(EntityTypeBuilder<AttendancePlanPeriod> builder)
    {
        builder.ToTable("PlanPeriods", "Attendance");

        builder
            .HasKey(period => period.Id);

        builder
            .Property(period => period.Id)
            .HasConversion(
                id => id.Value,
                value => AttendancePlanPeriodId.FromValue(value));

        builder
            .HasOne<AttendancePlan>()
            .WithMany(plan => plan.Periods)
            .HasForeignKey(period => period.PlanId);

        builder
            .HasOne<Period>()
            .WithMany()
            .HasForeignKey(period => period.PeriodId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(period => period.Timetable)
            .HasConversion(
                timetable => timetable.Code,
                code => Timetable.FromValue(code));

        builder
            .Property(period => period.Week)
            .HasConversion(
                week => week.Name,
                name => PeriodWeek.FromName(name));

        builder
            .Property(period => period.Day)
            .HasConversion(
                day => day.Name,
                name => PeriodDay.FromName(name));

        builder
            .Property(period => period.PeriodType)
            .HasConversion(
                type => type.Name,
                name => PeriodType.FromName(name));

        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(period => period.OfferingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Course>()
            .WithMany()
            .HasForeignKey(period => period.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(period => period.TargetMinutesPerCycle)
            .HasPrecision(2);
    }
}