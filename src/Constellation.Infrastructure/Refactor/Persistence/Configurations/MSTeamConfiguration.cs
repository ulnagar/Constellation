namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MSTeamConfiguration : IEntityTypeConfiguration<MSTeam>
{
    public void Configure(EntityTypeBuilder<MSTeam> builder)
    {
        builder.OwnsOne(team => team.TeamType);
    }
}
