namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.ThirdPartyConsent;

using Converters;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", "ThirdParty");

        builder
            .HasKey(transaction => transaction.Id);

        builder
            .Property(transaction => transaction.Id)
            .HasConversion(
                id => id.Value,
                value => ConsentTransactionId.FromValue(value));

        builder
            .Property(transaction => transaction.Responses)
            .HasConversion(new JsonColumnConverter<List<Transaction.ConsentResponse>>());
        
        builder
            .ComplexProperty(transaction => transaction.Student)
            .IsRequired();

        builder
            .ComplexProperty(transaction => transaction.Student)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(transaction => transaction.Student)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(transaction => transaction.Student)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(transaction => transaction.Method)
            .HasConversion(
                method => method.Value,
                value => ConsentMethod.FromValue(value));
    }
}