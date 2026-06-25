namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.ComplexTypes;

using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class MailingAddressConfiguration
{
    /// <summary>
    /// Configures a <see cref="MailingAddress"/> complex property with optional column name
    /// prefixing for tables that own more than one address.
    /// </summary>
    /// <example>
    /// Single address:
    ///   builder.ComplexProperty(e => e.MailingAddress).ConfigureMailingAddress();
    ///   → columns: Street, Town, State, Postcode
    ///
    /// Multiple addresses on the same table:
    ///   builder.ComplexProperty(e => e.HomeAddress).ConfigureMailingAddress("Home");
    ///   builder.ComplexProperty(e => e.PostalAddress).ConfigureMailingAddress("Postal");
    ///   → columns: Home_Street, Home_Town, ..., Postal_Street, Postal_Town, ...
    /// </example>
    public static ComplexPropertyBuilder<MailingAddress> ConfigureMailingAddress(
        this ComplexPropertyBuilder<MailingAddress> builder,
        string? columnPrefix = null)
    {
        string Col(string name) => columnPrefix is null ? name : $"{columnPrefix}_{name}";

        builder.Property(a => a.Street)
            .HasColumnName(Col("Street"))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Town)
            .HasColumnName(Col("Town"))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.State)
            .HasColumnName(Col("State"))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(a => a.Postcode)
            .HasColumnName(Col("Postcode"))
            .HasMaxLength(4)
            .IsRequired();

        return builder;
    }

    public static OwnedNavigationBuilder<TOwner, MailingAddress> ConfigureMailingAddress<TOwner>(
        this OwnedNavigationBuilder<TOwner, MailingAddress> builder,
        string? columnPrefix = null)
        where TOwner : class
    {
        string Col(string name) => columnPrefix is null ? name : $"{columnPrefix}_{name}";

        builder.Property(a => a.Street)
            .HasColumnName(Col("Street"))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Town)
            .HasColumnName(Col("Town"))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.State)
            .HasColumnName(Col("State"))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(a => a.Postcode)
            .HasColumnName(Col("Postcode"))
            .HasMaxLength(4)
            .IsRequired();

        return builder;
    }
}
