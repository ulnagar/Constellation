namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TimetableConfiguration : IEntityTypeConfiguration<Timetable>
{
    public void Configure(EntityTypeBuilder<Timetable> builder)
    {
        builder.HasMany(timetable => timetable.Periods).WithOne(period => period.Timetable).OnDelete(DeleteBehavior.NoAction);
    }
}
