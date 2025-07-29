namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Absences;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
{
    public void Configure(EntityTypeBuilder<Absence> builder)
    {
        builder.ToTable("Absences_Absences");

        builder
            .HasKey(absence => absence.Id);

        builder
            .Property(absence => absence.Id)
            .HasConversion(
                id => id.Value,
                value => AbsenceId.FromValue(value));
        
        builder
            .Property(absence => absence.Type)
            .HasConversion(
                entry => entry.Value,
                value => AbsenceType.FromValue(value));

        builder
            .Property(absence => absence.AbsenceReason)
            .HasConversion(
                entry => entry.Value,
                value => AbsenceReason.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(absence => absence.StudentId);

        builder
            .Property(absence => absence.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(absence => absence.OfferingId);

        builder
            .HasMany(absence => absence.Notifications)
            .WithOne()
            .HasForeignKey(notification => notification.AbsenceId);

        builder
            .Navigation(absence => absence.Notifications)
            .AutoInclude();

        builder
            .HasMany(absence => absence.Responses)
            .WithOne()
            .HasForeignKey(response => response.AbsenceId);

        builder.Navigation(absence => absence.Responses)
            .AutoInclude();
    }
}
