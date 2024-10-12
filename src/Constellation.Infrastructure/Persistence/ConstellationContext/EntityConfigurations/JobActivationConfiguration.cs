namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JobActivationConfiguration : IEntityTypeConfiguration<JobActivation>
{
    public void Configure(EntityTypeBuilder<JobActivation> builder)
    {
        builder.HasKey(job => job.Id);

        builder.HasIndex(job => job.JobName).IsUnique();
    }
}