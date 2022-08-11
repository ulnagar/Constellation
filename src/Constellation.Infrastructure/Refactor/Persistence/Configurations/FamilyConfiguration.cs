namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.HasMany(family => family.Students).WithOne(student => student.Family).OnDelete(DeleteBehavior.NoAction);

        builder.OwnsOne(family => family.Parent1);

        builder.OwnsOne(family => family.Parent2);

        builder.OwnsOne(family => family.Address);
    }
}
