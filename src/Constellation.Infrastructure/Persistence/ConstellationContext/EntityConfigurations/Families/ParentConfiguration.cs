namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Converters;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Families_Parents");

        builder
            .HasKey(parent => parent.Id);

        builder
            .Property(parent => parent.Id)
            .HasConversion(
                parentId => parentId.Value,
                value => ParentId.FromValue(value));

        builder
            .ComplexProperty(parent => parent.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName));

        builder
            .ComplexProperty(parent => parent.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName));

        builder
            .Property(parent => parent.EmailAddress)
            .HasConversion<EmailAddressConverter>();

        builder
            .Property(parent => parent.MobileNumber)
            .HasConversion<PhoneNumberConverter>();
    }
}
