using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class DeviceAllocationConfiguration : IEntityTypeConfiguration<DeviceAllocation>
    {
        public void Configure(EntityTypeBuilder<DeviceAllocation> builder)
        {
            builder.HasKey(d => d.Id);

            builder.HasOne(d => d.Student)
                .WithMany(s => s.Devices)
                .HasForeignKey(d => d.StudentId);

            builder.HasOne(d => d.Device)
                .WithMany(d => d.Allocations)
                .HasForeignKey(d => d.SerialNumber);
        }
    }
}