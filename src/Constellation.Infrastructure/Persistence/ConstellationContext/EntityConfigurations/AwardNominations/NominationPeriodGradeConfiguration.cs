namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AwardNominations;

using Constellation.Core.Models.Awards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NominationPeriodGradeConfiguration : IEntityTypeConfiguration<NominationPeriodGrade>
{
    public void Configure(EntityTypeBuilder<NominationPeriodGrade> builder)
    {
        builder.ToTable("PeriodGrades","AwardNominations");

        builder
            .HasKey(grade => new { grade.PeriodId, grade.Grade });
    }
}