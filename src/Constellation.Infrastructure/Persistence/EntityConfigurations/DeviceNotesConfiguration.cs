using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class DeviceNotesConfiguration : IEntityTypeConfiguration<DeviceNotes>
    {
        public void Configure(EntityTypeBuilder<DeviceNotes> builder)
        {
            builder.HasKey(n => n.Id);

            builder.HasOne(n => n.Device)
                .WithMany(d => d.Notes);
        }
    }
}