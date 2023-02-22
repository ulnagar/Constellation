namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Casuals;

using Constellation.Core.Models.Casuals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CasualConfiguration : IEntityTypeConfiguration<Casual>
{
    public void Configure(EntityTypeBuilder<Casual> builder)
    {
        builder.ToTable("Casuals_Casuals");

        builder
            .HasKey(casual => casual.Id);
    }
}