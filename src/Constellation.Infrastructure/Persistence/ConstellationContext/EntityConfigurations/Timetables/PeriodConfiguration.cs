namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Timetables;

using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class PeriodConfiguration : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> builder)
    {
        builder.ToTable("Periods", "Timetables");

        builder
            .HasKey(period => period.Id);

        builder
            .Property(period => period.Id)
            .HasConversion(
                id => id.Value,
                value => PeriodId.FromValue(value));

        builder
            .Property(period => period.Timetable)
            .HasConversion(
                timetable => timetable.Code,
                value => Timetable.FromValue(value));

        builder
            .Property(period => period.Day)
            .HasConversion(
                day => day.Name,
                value => PeriodDay.FromName(value));

        builder
            .Property(period => period.Type)
            .HasConversion(
                type => type.Name,
                value => PeriodType.FromName(value));
    }
}