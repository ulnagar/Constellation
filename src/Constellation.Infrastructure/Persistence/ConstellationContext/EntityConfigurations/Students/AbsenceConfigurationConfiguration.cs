namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AbsenceConfigurationConfiguration : IEntityTypeConfiguration<AbsenceConfiguration>
{
    public void Configure(EntityTypeBuilder<AbsenceConfiguration> builder) 
    {
        builder.ToTable("AbsenceConfigurations", "Students");

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
            .HasIndex(config => new { 
                config.StudentId, 
                config.AbsenceType, 
                config.CalendarYear });
    }
}
