namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("Families_Family");

        builder
            .HasKey(family => family.Id);

        builder
            .Property(family => family.Id)
            .HasConversion(
                familyId => familyId.Value,
                value => FamilyId.FromValue(value));

        builder
            .HasMany(family => family.Parents)
            .WithOne()
            .HasForeignKey(parent => parent.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(family => family.Parents)
            .AutoInclude();

        builder
            .HasMany(family => family.Students)
            .WithOne()
            .HasForeignKey(family => family.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(family => family.Students)
            .AutoInclude();
    }
}
