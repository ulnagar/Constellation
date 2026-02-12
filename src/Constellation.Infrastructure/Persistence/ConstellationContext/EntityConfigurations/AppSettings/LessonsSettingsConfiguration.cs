namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class LessonsSettingsConfiguration : IEntityTypeConfiguration<LessonsSettings>
{
    public void Configure(EntityTypeBuilder<LessonsSettings> builder)
    {
        builder.ToTable("Lessons", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(entry => entry.Supervisor)
            .HasConversion<JsonColumnConverter<IReadOnlyList<StaffMemberLink>>>();
    }
}