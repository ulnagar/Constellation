namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TimetablePeriodConfiguration : IEntityTypeConfiguration<TimetablePeriod>
{
    public void Configure(EntityTypeBuilder<TimetablePeriod> builder)
    {
        builder
            .HasKey(p => p.Id);

        builder
            .HasMany(p => p.OfferingSessions)
            .WithOne();
    }
}