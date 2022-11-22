using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

public class FacultyMembershipConfiguration : IEntityTypeConfiguration<FacultyMembership>
{
    public void Configure(EntityTypeBuilder<FacultyMembership> builder)
    {
        builder.HasKey(member => member.Id);

        builder.HasOne(member => member.Faculty)
            .WithMany(faculty => faculty.Members)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(member => member.Staff)
            .WithMany(staff => staff.Faculties)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
