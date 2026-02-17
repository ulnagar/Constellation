namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Converters;
using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MandatoryTrainingSettingsConfiguration : IEntityTypeConfiguration<MandatoryTrainingSettings>
{
    public void Configure(EntityTypeBuilder<MandatoryTrainingSettings> builder)
    {
        builder.ToTable("MandatoryTraining", "AppSettings");

        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");
        
        builder
            .Property(entry => entry.Contacts)
            .HasConversion<JsonColumnConverter<List<StaffMemberLink>>>();
    }
}