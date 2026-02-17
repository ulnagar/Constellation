namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Core.Models.AppSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class WorkflowSettingsConfiguration : IEntityTypeConfiguration<WorkflowSettings>
{
    public void Configure(EntityTypeBuilder<WorkflowSettings> builder)
    {
        builder.ToTable("Workflows", "AppSettings");

        builder
            .HasKey(entry => entry.PositionName);

        builder
            .Property(entry => entry.PositionName)
            .HasConversion(
                position => position.Value,
                value => WorkflowArea.FromValue(value));

        builder
            .Property(entry => entry.Members)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}