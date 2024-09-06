namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Constellation.Core.Models.Students;
using Core.Models.Absences;
using Core.Models.Families;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students", "Students");

        builder
            .HasKey(student => student.Id);

        builder
            .Property(student => student.Id)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));

        builder
            .Property(student => student.StudentReferenceNumber)
            .IsRequired(false)
            .HasConversion(
                entry => entry.Number,
                value => StudentReferenceNumber.FromValue(value));

        builder
            .HasIndex(student => student.StudentReferenceNumber)
            .IsUnique();

        //builder
        //    .OwnsOne(student => student.Name);

        builder
            .ComplexProperty(student => student.Name)
            .IsRequired();

        builder
            .ComplexProperty(student => student.Name)
            .Property(name => name.FirstName)
            .IsRequired();

        builder
            .ComplexProperty(student => student.Name)
            .Property(name => name.PreferredName)
            .IsRequired(false);

        builder
            .ComplexProperty(student => student.Name)
            .Property(name => name.LastName)
            .IsRequired();

        builder
            .Property(student => student.EmailAddress)
            .HasConversion(
                email => email.Email,
                value => EmailAddress.FromValue(value));

        builder
            .Property(s => s.Gender)
            .HasConversion(
                gender => gender.Value,
                value => Gender.FromValue(value));

        builder
            .Property(student => student.PreferredGender)
            .HasConversion(
                gender => gender.Value,
                value => Gender.FromValue(value));

        builder
            .HasOne(student => student.AwardTally)
            .WithOne()
            .HasForeignKey<AwardTally>(tally => tally.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(student => student.AwardTally)
            .AutoInclude();

        builder
            .HasMany(student => student.AbsenceConfigurations)
            .WithOne()
            .HasForeignKey(config => config.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(student => student.AbsenceConfigurations)
            .AutoInclude();

        builder
            .HasMany(student => student.SchoolEnrolments)
            .WithOne()
            .HasForeignKey(enrolment => enrolment.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(student => student.SchoolEnrolments)
            .AutoInclude();

        builder
            .HasMany(student => student.SystemLinks)
            .WithOne()
            .HasForeignKey(link => link.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(student => student.SystemLinks)
            .AutoInclude();

        builder
            .HasMany<Absence>()
            .WithOne()
            .HasForeignKey(absence => absence.StudentId);
    }
}