namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Audit;

using Constellation.Application.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable("Audit");

        builder
            .HasKey(audit => audit.Id);
    }
}
