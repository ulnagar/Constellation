namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Core.Models.AppSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ContactsSettingsConfiguration : IEntityTypeConfiguration<ContactsSettings>
{
    public void Configure(EntityTypeBuilder<ContactsSettings> builder)
    {
        builder.ToTable("Contacts", "AppSettings");

        builder
            .HasKey(entry => entry.PositionName);

        builder
            .Property(entry => entry.PositionName)
            .HasConversion(
                name => name.Value,
                value => ContactPosition.FromValue(value));

        builder
            .Property(entry => entry.Members)
            .HasConversion<JsonColumnConverter<IReadOnlyList<StaffMemberLink>>>();
    }
}