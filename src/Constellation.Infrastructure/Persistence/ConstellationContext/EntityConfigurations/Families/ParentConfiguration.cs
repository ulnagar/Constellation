namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Families_Parents");

        builder
            .HasKey(parent => parent.Id);

        builder
            .Property(parent => parent.Id)
            .HasConversion(
                parentId => parentId.Value,
                value => ParentId.FromValue(value));

        builder
            .HasOne<Family>()
            .WithMany(family => family.Parents)
            .HasForeignKey(parent => parent.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
