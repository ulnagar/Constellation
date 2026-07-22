namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.Application;

using ComplexTypes;
using ConstellationContext.Converters;
using Converters;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");

        builder
            .HasKey(entry => entry.Id);

        builder
            .HasOne<EnrolmentPeriod>()
            .WithMany()
            .HasForeignKey(entry => entry.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(entry => entry.StudentReferenceNumber)
            .HasConversion<StudentReferenceNumberConverter>()
            .IsRequired(false);

        builder
            .ComplexProperty(entry => entry.StudentName)
            .IsRequired()
            .ConfigureName("Student");

        builder
            .Property(entry => entry.StudentGender)
            .IsRequired()
            .HasConversion<GenderConverter>();

        builder
            .Property(entry => entry.DateOfBirth)
            .IsRequired(false);

        builder
            .Property(entry => entry.StudentEmailAddress)
            .HasConversion<EmailAddressConverter>()
            .IsRequired(false);

        builder
            .OwnsOne(
                entry => entry.ParentName,
                owned => owned.ConfigureName("Parent"));

        builder
            .Property(entry => entry.ParentEmailAddress)
            .HasConversion<EmailAddressConverter>()
            .IsRequired(false);

        builder
            .Property(entry => entry.ParentPhoneNumber)
            .HasConversion<PhoneNumberConverter>()
            .IsRequired(false);

        builder
            .OwnsOne(
                entry => entry.MailingAddress,
                owned => owned.ConfigureMailingAddress());

        builder
            .Property(entry => entry.CurrentSchoolCode)
            .IsRequired(false)
            .HasConversion<StronglyTypedIdValueConverter<SchoolCode, string>>();

        builder
            .Property(entry => entry.DestinationSchoolCode)
            .IsRequired(false)
            .HasConversion<StronglyTypedIdValueConverter<SchoolCode, string>>();

        builder
            .Property(entry => entry.Program)
            .HasConversion<ProgramConverter>();

        builder
            .Property(entry => entry.Grade)
            .HasConversion<string>();

        builder
            .Property(entry => entry.Status)
            .HasConversion<string>();
    }
}