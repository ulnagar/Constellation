namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.ThirdPartyConsent;

using Core.Models.Students;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
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
            .Property(transaction => transaction.SubmissionMethod)
            .HasConversion(
                method => method.Value,
                value => ConsentMethod.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(transaction => transaction.StudentId)
            .IsRequired();

        builder
            .Navigation(transaction => transaction.Consents)
            .AutoInclude();
    }
}