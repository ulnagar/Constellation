using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class SchoolContactConfiguration : IEntityTypeConfiguration<SchoolContact>
    {
        public void Configure(EntityTypeBuilder<SchoolContact> builder)
        {
            builder.ToTable("SchoolContact");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(10);
        }
    }
}