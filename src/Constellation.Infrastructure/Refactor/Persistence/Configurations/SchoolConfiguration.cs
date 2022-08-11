namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.HasMany(school => school.Students).WithOne(student => student.School).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(school => school.StaffMembers).WithOne(staff => staff.School).OnDelete(DeleteBehavior.NoAction);
    }
}
