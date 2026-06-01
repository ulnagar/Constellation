namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StudentOnboarding;

using Converters;
using Core.Models.StudentOnboarding;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed record ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents", "Onboarding");

        builder
            .HasKey(entry => entry.Id);

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
            .Property(entry => entry.MobileNumber)
            .HasConversion<PhoneNumberConverter>();
    }
}