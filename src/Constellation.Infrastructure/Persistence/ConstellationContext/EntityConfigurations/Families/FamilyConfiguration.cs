namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("Families_Family");

        builder
            .HasKey(s => s.Id);

        builder
            .HasMany(s => s.Parents)
            .WithOne()
            .HasForeignKey(p => p.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(s => s.Students)
            .WithOne()
            .HasForeignKey(m => m.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
