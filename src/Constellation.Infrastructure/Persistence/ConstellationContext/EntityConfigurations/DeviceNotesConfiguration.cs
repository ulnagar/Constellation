namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DeviceNotesConfiguration : IEntityTypeConfiguration<DeviceNotes>
{
    public void Configure(EntityTypeBuilder<DeviceNotes> builder)
    {
        builder.HasKey(n => n.Id);

        builder.HasOne(n => n.Device)
            .WithMany(d => d.Notes);
    }
}