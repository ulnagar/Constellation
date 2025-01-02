namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attendance;

using Core.Models.Attendance;
using Core.Models.Attendance.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AttendancePlanNoteConfiguration : IEntityTypeConfiguration<AttendancePlanNote>
{
    public void Configure(EntityTypeBuilder<AttendancePlanNote> builder)
    {
        builder.ToTable("Notes", "Attendance");

        builder
            .HasKey(note => note.Id);

        builder
            .Property(note => note.Id)
            .HasConversion(
                id => id.Value,
                value => AttendancePlanNoteId.FromValue(value));

        builder
            .Property(note => note.Message)
            .IsRequired();

        builder
            .Property(note => note.PlanId)
            .IsRequired();
    }
}
