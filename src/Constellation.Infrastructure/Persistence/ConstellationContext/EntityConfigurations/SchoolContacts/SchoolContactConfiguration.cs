namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SchoolContacts;

using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.ValueObjects;
using Converters;
using Core.Models.SchoolContacts.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SchoolContactConfiguration : IEntityTypeConfiguration<SchoolContact>
{
    public void Configure(EntityTypeBuilder<SchoolContact> builder)
    {
        builder.ToTable("SchoolContacts_Contacts");

        builder
            .HasKey(contact => contact.Id);

        builder
            .Property(contact => contact.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolContactId.FromValue(value));

        builder
            .ComplexProperty(contact => contact.Name)
            .IsRequired();

        builder
            .ComplexProperty(contact => contact.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(student => student.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(student => student.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(contact => contact.PhoneNumber)
            .HasConversion<PhoneNumberConverter>();

        builder
            .Property(contact => contact.EmailAddress)
            .HasConversion<EmailAddressConverter>();

        builder
            .HasMany(contact => contact.Assignments)
            .WithOne()
            .HasForeignKey(role => role.SchoolContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(contact => contact.Assignments)
            .AutoInclude();
    }
}