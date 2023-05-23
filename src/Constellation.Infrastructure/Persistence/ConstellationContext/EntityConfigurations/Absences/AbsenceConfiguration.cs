﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Absences;

using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
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
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(absence => absence.StudentId);

        builder
            .HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(absence => absence.OfferingId);

        builder
            .HasMany(absence => absence.Notifications)
            .WithOne();

        builder
            .HasMany(absence => absence.Responses)
            .WithOne();
    }
}
