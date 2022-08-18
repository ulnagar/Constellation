using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class StudentFamilyConfiguration : IEntityTypeConfiguration<StudentFamily>
    {
        public void Configure(EntityTypeBuilder<StudentFamily> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasMany(s => s.Students).WithOne(s => s.Family);

            builder.OwnsOne(s => s.Parent1);
            builder.OwnsOne(s => s.Parent2);
            builder.OwnsOne(s => s.Address);
        }
    }
}