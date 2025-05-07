namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EdvalClassMembershipConfiguration : IEntityTypeConfiguration<EdvalClassMembership>
{
    public void Configure(EntityTypeBuilder<EdvalClassMembership> builder)
    {
        builder.ToTable("ClassMembership", "Edval");

        builder
            .HasKey(entity => new { entity.StudentId, entity.EdvalClassCode });
    }
}