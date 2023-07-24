namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal sealed class AbsenceConfigurationConfiguration : IEntityTypeConfiguration<AbsenceConfiguration>
{
    public void Configure(EntityTypeBuilder<AbsenceConfiguration> builder) 
    {
        builder.ToTable("Students_AbsenceConfiguration");

        builder
            .HasKey(config => config.Id);

        builder
            .Property(config => config.Id)
            .HasConversion(
                configId => configId.Value,
                value => StudentAbsenceConfigurationId.FromValue(value));

        builder
            .Property(config => config.AbsenceType)
            .HasConversion(
                entry => entry.Value,
                value => AbsenceType.FromValue(value));

        builder
            .Property(config => config.ScanStartDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(config => config.ScanEndDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .HasIndex(config => new { 
                config.StudentId, 
                config.AbsenceType, 
                config.CalendarYear });
    }
}
