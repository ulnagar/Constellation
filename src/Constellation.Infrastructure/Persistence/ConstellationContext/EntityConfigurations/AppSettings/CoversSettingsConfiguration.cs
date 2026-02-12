namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CoversSettingsConfiguration : IEntityTypeConfiguration<CoversSettings>
{
    public void Configure(EntityTypeBuilder<CoversSettings> builder)
    {
        builder.ToTable("Covers", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(entry => entry.Supervisor)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}