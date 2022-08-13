namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.HasOne(faculty => faculty.ManagementUnit).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(faculty => faculty.HeadTeacher).WithOne().OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(faculty => faculty.StaffMembers).WithMany(staff => staff.Faculties);

        builder.HasMany(faculty => faculty.Resources).WithOne(resource => resource.Faculty);
    }
}
