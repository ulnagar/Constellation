namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AwardTallyConfiguration: IEntityTypeConfiguration<AwardTally>
{
    public void Configure(EntityTypeBuilder<AwardTally> builder)
    {
        builder.ToTable("AwardTallies", "Students");

        builder
            .HasKey(tally => tally.StudentId);
    }
}