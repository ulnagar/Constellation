namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClassAssignmentConfiguration : IEntityTypeConfiguration<ClassAssignment>
{
    public void Configure(EntityTypeBuilder<ClassAssignment> builder)
    {
        builder.HasOne(assignment => assignment.Class).WithMany(@class => @class.Teachers).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(assignment => assignment.StaffMember).WithMany(staff => staff.Assignments).OnDelete(DeleteBehavior.NoAction);
    }
}
