using Constellation.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class JobActivationConfiguration : IEntityTypeConfiguration<JobActivation>
    {
        public void Configure(EntityTypeBuilder<JobActivation> builder)
        {
            builder.HasKey(job => job.Id);

            builder.HasIndex(job => job.JobName).IsUnique();
        }
    }
}
