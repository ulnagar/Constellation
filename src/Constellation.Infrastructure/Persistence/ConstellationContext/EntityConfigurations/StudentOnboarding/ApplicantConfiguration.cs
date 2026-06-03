namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StudentOnboarding;

using Constellation.Infrastructure.Persistence.ConstellationContext.Converters;
using Core.Models.Common.Enums;
using Core.Models.StudentOnboarding;
using Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> builder)
    {
        builder.ToTable("Applicants", "Onboarding");

        builder
            .HasKey(entry => entry.Id);

        builder
            .Property(entry => entry.StudentReferenceNumber)
            .IsRequired(false)
            .HasConversion<StudentReferenceNumberConverter>();

        builder
            .ComplexProperty(entry => entry.Name)
            .ApplyConfiguration()
            .IsRequired();

        builder
            .Property(entry => entry.EmailAddress)
            .HasConversion<EmailAddressConverter>()
            .IsRequired(false);

        builder
            .Property(entry => entry.Gender)
            .HasConversion(
                gender => gender.Value,
                value => Gender.FromValue(value))
            .IsRequired(false);

        builder
            .Property(entry => entry.IndigenousStatus)
            .HasConversion(
                status => status.Value,
                value => IndigenousStatus.FromValue(value))
            .IsRequired(false);
    }
}