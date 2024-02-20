using Constellation.Core.Models.SchoolContacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
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