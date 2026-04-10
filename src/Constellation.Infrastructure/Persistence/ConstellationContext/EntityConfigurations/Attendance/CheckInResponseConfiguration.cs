namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attendance;

using Core.Models.Attendance.Checkin;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Students;
using Core.Models.Subjects;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CheckInResponseConfiguration : IEntityTypeConfiguration<CheckInResponse>
{
    public void Configure(EntityTypeBuilder<CheckInResponse> builder)
    {
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("CheckInResponses", "Attendance");

        builder
            .HasKey(response => new { response.StudentId, response.SubmittedAt });

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(response => response.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(response => response.OfferingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Course>()
            .WithMany()
            .HasForeignKey(response => response.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .ComplexProperty(response => response.Student)
            .IsRequired();

        builder
            .ComplexProperty(response => response.Student)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(response => response.Student)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(response => response.Student)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(response => response.Offering)
            .HasConversion(
                name => name.Value,
                value => OfferingName.FromValue(value));

        builder
            .Property(response => response.SchoolCode)
            .HasConversion(
                id => id.Value,
                value => SchoolCode.FromValue(value));
    }
}