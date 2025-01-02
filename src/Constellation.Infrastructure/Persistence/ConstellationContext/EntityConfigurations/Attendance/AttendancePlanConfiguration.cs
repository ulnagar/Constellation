namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attendance;

using Constellation.Infrastructure.Persistence.ConstellationContext.Converters;
using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Students;
using Core.Models.Timetables.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AttendancePlanConfiguration : IEntityTypeConfiguration<AttendancePlan>
{
    public void Configure(EntityTypeBuilder<AttendancePlan> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Plans", "Attendance");

        builder
            .HasKey(plan => plan.Id);

        builder
            .Property(plan => plan.Id)
            .HasConversion(
                id => id.Value,
                value => AttendancePlanId.FromValue(value));

        builder
            .Property(plan => plan.Status)
            .HasConversion(
                status => status.Value,
                value => AttendancePlanStatus.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(plan => plan.StudentId);

        builder
            .HasMany<AttendancePlanNote>()
            .WithOne()
            .HasForeignKey(note => note.PlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .Property(plan => plan.SciencePracLesson)
            .IsRequired(false)
            .HasConversion(new JsonColumnConverter<AttendancePlanSciencePracLesson>());

        builder
            .OwnsMany<AttendancePlanFreePeriod>(nameof(AttendancePlan.FreePeriods), config =>
            {
                config.ToJson();

                config
                    .Property(period => period.Week)
                    .HasConversion(
                        week => week.Name,
                        name => PeriodWeek.FromName(name));

                config
                    .Property(period => period.Day)
                    .HasConversion(
                        day => day.Name,
                        name => PeriodDay.FromName(name));
            });

        builder
            .OwnsMany<AttendancePlanMissedLesson>(nameof(AttendancePlan.MissedLessons), config =>
            {
                config.ToJson();
            });

        builder
            .ComplexProperty(plan => plan.Student)
            .IsRequired();

        builder
            .ComplexProperty(plan => plan.Student)
            .Property(name => name.FirstName)
            .IsRequired();

        builder
            .ComplexProperty(plan => plan.Student)
            .Property(name => name.PreferredName)
            .IsRequired(false);

        builder
            .ComplexProperty(plan => plan.Student)
            .Property(name => name.LastName)
            .IsRequired();

        builder
            .Property(plan => plan.SchoolCode)
            .HasMaxLength(4);

        builder
            .Navigation(plan => plan.Periods)
            .AutoInclude();

        builder
            .Navigation(plan => plan.Notes)
            .AutoInclude();
    }
}