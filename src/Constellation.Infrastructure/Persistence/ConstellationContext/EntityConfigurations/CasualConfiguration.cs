using Constellation.Core.Models.Covers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class CasualConfiguration : IEntityTypeConfiguration<Casual>
    {
        public void Configure(EntityTypeBuilder<Casual> builder)
        {
            builder.HasMany(c => c.ClassCovers)
                .WithOne(v => v.Casual)
                .HasForeignKey(c => c.CasualId);
        }
    }
}