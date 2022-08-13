namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.HasMany(grade => grade.Students).WithMany(student => student.Cohorts);

        builder.HasMany(grade => grade.Resources).WithOne(resource => resource.Grade).OnDelete(DeleteBehavior.NoAction);
    }
}
