namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SciencePracs;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SciencePracAttendanceConfiguration : IEntityTypeConfiguration<SciencePracAttendance>
{
    public void Configure(EntityTypeBuilder<SciencePracAttendance> builder)
    {
        builder.ToTable("SciencePracs_Attendance");

        builder
            .HasKey(roll => roll.Id);

        builder
            .Property(roll => roll.Id)
            .HasConversion(
                id => id.Value,
                value => SciencePracAttendanceId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(attendance => attendance.StudentId);
    }
}
