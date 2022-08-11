namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Constellation.Infrastructure.Refactor.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PeriodConfiguration : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> builder)
    {
        builder.OwnsOne(period => period.PeriodType);

        builder.HasMany(period => period.Sessions).WithOne(session => session.Period).OnDelete(DeleteBehavior.NoAction);

        builder.Property(period => period.StartTime).HasConversion<TimeOnlyConverter, TimeOnlyComparer>();
        
        builder.Property(period => period.EndTime).HasConversion<TimeOnlyConverter, TimeOnlyComparer>();
    }
}
