namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CanvasSettingsConfiguration : IEntityTypeConfiguration<CanvasSettings>
{
    public void Configure(EntityTypeBuilder<CanvasSettings> builder)
    {
        builder.ToTable("Canvas", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(entry => entry.Admins)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}