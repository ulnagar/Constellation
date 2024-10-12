namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DeviceAllocationConfiguration : IEntityTypeConfiguration<DeviceAllocation>
{
    public void Configure(EntityTypeBuilder<DeviceAllocation> builder)
    {
        builder.ToTable("DeviceAllocations");

        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.Student)
            .WithMany()
            .HasForeignKey(d => d.StudentId);

        builder.HasOne(d => d.Device)
            .WithMany(d => d.Allocations)
            .HasForeignKey(d => d.SerialNumber);
    }
}