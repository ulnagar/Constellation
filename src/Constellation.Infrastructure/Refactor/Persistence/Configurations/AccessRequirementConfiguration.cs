namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccessRequirementConfiguration : IEntityTypeConfiguration<AccessRequirement>
{
    public void Configure(EntityTypeBuilder<AccessRequirement> builder)
    {
        builder.HasOne(access => access.SystemResource).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}
