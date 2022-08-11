namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.HasOne(session => session.Class).WithMany(@class => @class.Sessions).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(session => session.Period).WithMany(period => period.Sessions).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(session => session.StaffMember).WithMany(staff => staff.Sessions).OnDelete(DeleteBehavior.NoAction);
    }
}
