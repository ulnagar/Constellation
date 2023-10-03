namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Offerings;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Offerings_Sessions");

        builder
            .HasKey(session => session.Id);

        builder
            .Property(session => session.Id)
            .HasConversion(
                id => id.Value,
                value => SessionId.FromValue(value));

        builder
            .HasOne(session => session.Offering)
            .WithMany(offering => offering.Sessions)
            .HasForeignKey(session => session.OfferingId);

        builder
            .Property(session => session.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .HasOne<TimetablePeriod>()
            .WithMany(period => period.OfferingSessions)
            .HasForeignKey(session => session.PeriodId);
    }
}