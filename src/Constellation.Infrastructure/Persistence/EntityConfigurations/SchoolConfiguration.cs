using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class SchoolConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            builder.HasKey(s => s.Code);

            builder.Property(s => s.Code)
                .HasMaxLength(4);

            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(10);

            builder.Property(s => s.FaxNumber)
                .HasMaxLength(10);

            builder.HasMany(s => s.Students)
                .WithOne(s => s.School)
                .HasForeignKey(s => s.SchoolCode)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(s => s.Staff)
                .WithOne(s => s.School)
                .HasForeignKey(s => s.SchoolCode)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(s => s.StaffAssignments)
                .WithOne(a => a.School)
                .HasForeignKey(a => a.SchoolCode)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}