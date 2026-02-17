namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialsSettingsConfiguration : IEntityTypeConfiguration<TutorialsSettings>
{
    public void Configure(EntityTypeBuilder<TutorialsSettings> builder)
    {
        builder.ToTable("Tutorials", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(entry => entry.Members)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}