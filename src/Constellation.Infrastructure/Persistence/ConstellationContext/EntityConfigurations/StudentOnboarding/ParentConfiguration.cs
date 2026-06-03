namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StudentOnboarding;

using Converters;
using Core.Models.StudentOnboarding;
using Extensions;
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
            .ApplyConfiguration()
            .IsRequired();

        builder
            .Property(entry => entry.EmailAddress)
            .HasConversion<EmailAddressConverter>()
            .IsRequired(false);

        builder
            .Property(entry => entry.MobileNumber)
            .HasConversion<PhoneNumberConverter>()
            .IsRequired(false);

        builder
            .Property(entry => entry.MailingAddress)
            .HasConversion<MailingAddressConverter>()
            .IsRequired(false);
    }
}