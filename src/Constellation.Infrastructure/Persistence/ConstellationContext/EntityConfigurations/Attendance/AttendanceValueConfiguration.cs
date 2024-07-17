namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attendance;

using Core.Models.Attendance;
using Core.Models.Attendance.Identifiers;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AttendanceValueConfiguration
    : IEntityTypeConfiguration<AttendanceValue>
{
    public void Configure(EntityTypeBuilder<AttendanceValue> builder)
    {
        builder.ToTable("Attendance_Values");

        builder
            .HasKey(value => value.Id);

        builder
            .Property(value => value.Id)
            .HasConversion(
                id => id.Value,
                value => AttendanceValueId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(value => value.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(value => value.StartDate)
            .IsRequired();

        builder
            .Property(value => value.EndDate)
            .IsRequired();

        builder
            .Property(value => value.PerMinuteYearToDatePercentage)
            .HasPrecision(5,2)
            .IsRequired();

        builder
            .Property(value => value.PerMinuteWeekPercentage)
            .HasPrecision(5,2)
            .IsRequired();

        builder
            .Property(value => value.PerDayYearToDatePercentage)
            .HasPrecision(5,2)
            .IsRequired();

        builder
            .Property(value => value.PerDayWeekPercentage)
            .HasPrecision(5, 2)
            .IsRequired();
    }
}
