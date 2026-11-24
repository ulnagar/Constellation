namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.EnrolmentUser;

using Constellation.Core.ValueObjects;
using ConstellationContext.Converters;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EnrolmentUserConfiguration : IEntityTypeConfiguration<EnrolmentUser>
{
    public void Configure(EntityTypeBuilder<EnrolmentUser> builder)
    {
        builder
            .ComplexProperty(user => user.Name)
            .IsRequired();

        builder
            .ComplexProperty(user => user.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(user => user.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(user => user.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(user => user.PendingEmail)
            .HasConversion<EmailAddressConverter>();

        builder
            .Property(user => user.PendingPhone)
            .HasConversion<PhoneNumberConverter>();

        builder
            .HasMany(user => user.Logins)
            .WithOne()
            .HasForeignKey(user => user.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(user => user.Logins)
            .AutoInclude();

        builder
            .Navigation(user => user.PasskeyCredentials)
            .AutoInclude();
    }
}