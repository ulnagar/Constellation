namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Families_Parents");

        builder
            .HasKey(s => s.Id);
    }
}
