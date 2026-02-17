namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.Absences.Enums;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AbsencesSettingsConfiguration : IEntityTypeConfiguration<AbsencesSettings>
{
    public void Configure(EntityTypeBuilder<AbsencesSettings> builder)
    {
        builder.ToTable("Absences", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");

        builder
            .Property(entry => entry.DiscountedWholeReasons)
            .HasConversion<JsonColumnConverter<List<AbsenceReason>>>();

        builder
            .Property(entry => entry.DiscountedPartialReasons)
            .HasConversion<JsonColumnConverter<List<AbsenceReason>>>();

        builder
            .Property(entry => entry.RollMarkingReportRecipients)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}