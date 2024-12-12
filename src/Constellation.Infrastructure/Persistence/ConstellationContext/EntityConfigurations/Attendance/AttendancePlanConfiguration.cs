namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attendance;

using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AttendancePlanConfiguration : IEntityTypeConfiguration<AttendancePlan>
{
    public void Configure(EntityTypeBuilder<AttendancePlan> builder)
    {
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
    }
}