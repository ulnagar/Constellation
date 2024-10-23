namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.ThirdPartyConsent;

using Constellation.Core.Models.ThirdPartyConsent;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.Students;
using Core.Models.ThirdPartyConsent.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class ConsentConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        builder.ToTable("Consents", "ThirdParty");

        builder
            .HasKey(consent => consent.Id);

        builder
            .Property(consent => consent.Id)
            .HasConversion(
                id => id.Value,
                value => ConsentId.FromValue(value));

        builder
            .Property(consent => consent.ApplicationId)
            .HasConversion(
                id => id.Value,
                value => ApplicationId.FromValue(value));

        builder
            .HasOne<Application>()
            .WithMany(application => application.Consents)
            .HasForeignKey(consent => consent.ApplicationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(consent => consent.TransactionId)
            .HasConversion(
                id => id.Value,
                value => ConsentTransactionId.FromValue(value));

        builder
            .HasOne<Transaction>()
            .WithMany(transaction => transaction.Consents)
            .HasForeignKey(consent => consent.TransactionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
            
        builder
            .Property(consent => consent.Method)
            .HasConversion(
                method => method.Value,
                value => ConsentMethod.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(consent => consent.StudentId)
            .IsRequired();
    }
}
