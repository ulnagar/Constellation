namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StudentOnboarding;

using Constellation.Infrastructure.Persistence.ConstellationContext.Converters;
using Core.Models;
using Core.Models.Common.Enums;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Enums;
using Core.ValueObjects;
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
            .HasConversion(new StudentReferenceNumberConverter());

        builder
            .ComplexProperty(entry => entry.Name)
            .IsRequired();

        builder
            .ComplexProperty(entry => entry.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(entry => entry.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(entry => entry.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(entry => entry.EmailAddress)
            .HasConversion<EmailAddressConverter>();

        builder
            .Property(entry => entry.Gender)
            .HasConversion(
                gender => gender.Value,
                value => Gender.FromValue(value));

        builder
            .Property(entry => entry.IndigenousStatus)
            .HasConversion(
                status => status.Value,
                value => IndigenousStatus.FromValue(value));

        builder
            .Property(entry => entry.Program)
            .HasConversion(
                program => program.Value,
                value => Program.FromValue(value));

        builder
            .Property(entry => entry.Year)
            .HasMaxLength(4);
        
        builder
            .HasMany(entry => entry.Parents)
            .WithOne()
            .HasForeignKey(parent => parent.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(entry => entry.Parents)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_parents")
            .AutoInclude();

        builder
            .HasOne<School>()
            .WithMany()
            .HasForeignKey(entry => entry.SchoolCode)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}