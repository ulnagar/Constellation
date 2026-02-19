namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Core.Models.AppSettings;
using Core.Models.AppSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SentralSettingsConfiguration : IEntityTypeConfiguration<SentralSettings>
{
    public void Configure(EntityTypeBuilder<SentralSettings> builder)
    {
        builder.ToTable("Sentral", "AppSettings");

        builder
            .HasKey(entry => entry.Type);

        builder
            .Property(entry => entry.Type)
            .HasConversion(
                type => type.Value,
                value => SentralPath.FromValue(value));

        builder
            .Property(entry => entry.Path)
            .IsRequired();
    }
}