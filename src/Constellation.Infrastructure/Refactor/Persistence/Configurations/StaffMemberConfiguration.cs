namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.OwnsOne(staff => staff.Gender);

        builder.HasOne(staff => staff.Photo).WithOne(photo => photo.StaffMember).HasForeignKey<StaffPhoto>(photo => photo.StaffMemberId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(staff => staff.Faculties).WithMany(faculty => faculty.StaffMembers);

        builder.HasOne(staff => staff.School).WithMany(school => school.StaffMembers).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(staff => staff.Sessions).WithOne(session => session.StaffMember).OnDelete(DeleteBehavior.NoAction);
    }
}
