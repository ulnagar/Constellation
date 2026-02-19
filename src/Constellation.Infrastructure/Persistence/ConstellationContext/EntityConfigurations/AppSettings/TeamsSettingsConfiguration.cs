namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TeamsSettingsConfiguration : IEntityTypeConfiguration<TeamsSettings>
{
    public void Configure(EntityTypeBuilder<TeamsSettings> builder)
    {
        builder.ToTable("Teams", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(entry => entry.MandatoryOwners)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();

        builder
            .Property(entry => entry.StudentTeamOwners)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();

        builder
            .Property(entry => entry.StudentChannelOwners)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}