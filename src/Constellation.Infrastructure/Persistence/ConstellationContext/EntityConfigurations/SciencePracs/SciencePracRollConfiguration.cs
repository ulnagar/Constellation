namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SciencePracs;

using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SciencePracRollConfiguration : IEntityTypeConfiguration<SciencePracRoll>
{
    public void Configure(EntityTypeBuilder<SciencePracRoll> builder)
    {
        builder.ToTable("SciencePracs_Rolls");

        builder
            .HasKey(roll => roll.Id);

        builder
            .Property(roll => roll.Id)
            .HasConversion(
                id => id.Value,
                value => SciencePracRollId.FromValue(value));

        builder
            .Property(roll => roll.LessonDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .HasOne<School>()
            .WithMany()
            .HasForeignKey(roll => roll.SchoolCode);

        builder
            .HasMany(roll => roll.Attendance)
            .WithOne()
            .HasForeignKey(attendance => attendance.SciencePracRollId);

        builder
            .Navigation(roll => roll.Attendance)
            .AutoInclude();
    }
}
