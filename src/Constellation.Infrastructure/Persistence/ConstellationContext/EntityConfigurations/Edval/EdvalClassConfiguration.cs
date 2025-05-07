namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EdvalClassConfiguration : IEntityTypeConfiguration<EdvalClass>
{
    public void Configure(EntityTypeBuilder<EdvalClass> builder)
    {
        builder.ToTable("Class", "Edval");

        builder
            .HasKey(entity => entity.EdvalClassCode);
    }
}