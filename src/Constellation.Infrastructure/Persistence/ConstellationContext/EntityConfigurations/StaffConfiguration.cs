using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            builder.ToTable("Staff");

            builder.HasKey(s => s.StaffId);

            builder.HasOne(s => s.School)
                .WithMany(s => s.Staff);

            builder.HasMany(staff => staff.TrainingCompletionRecords)
                .WithOne(completion => completion.Staff)
                .HasPrincipalKey(completion => completion.StaffId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}