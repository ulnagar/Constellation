namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.Application;

using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.Offer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");

        builder
            .HasKey(entry => entry.Id);

        builder
            .HasOne<EnrolmentPeriod>()
            .WithMany()
            .HasForeignKey(entry => entry.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Application>()
            .WithOne()
            .HasForeignKey<Offer>(entry => entry.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(entry => entry.Status)
            .HasConversion<string>();
    }
}