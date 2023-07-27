namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NominationPeriodConfiguration
    : IEntityTypeConfiguration<NominationPeriod>
{
    public void Configure(EntityTypeBuilder<NominationPeriod> builder)
    {
        builder.ToTable("Awards_NominationPeriods");

        builder
            .HasKey(period => period.Id);

        builder
            .Property(period => period.Id)
            .HasConversion(
                id => id.Value,
                value => AwardNominationPeriodId.FromValue(value));

        builder
            .Property(period => period.LockoutDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .HasMany(period => period.Nominations)
            .WithOne()
            .HasForeignKey(nomination => nomination.PeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(period => period.Nominations)
            .AutoInclude();
    }
}
