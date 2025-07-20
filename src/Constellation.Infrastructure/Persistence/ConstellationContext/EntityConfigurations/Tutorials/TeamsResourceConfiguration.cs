namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Tutorials;

using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TeamsResourceConfiguration : IEntityTypeConfiguration<TeamsResource>
{
    public void Configure(EntityTypeBuilder<TeamsResource> builder)
    {
        builder.ToTable("Teams", "Tutorials");

        builder
            .HasKey(team => team.Id);

        builder
            .Property(team => team.Id)
            .HasConversion(
                id => id.Value,
                value => TeamsResourceId.FromValue(value));
    }
}