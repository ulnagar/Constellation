using Constellation.Core.Models.Faculty;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Faculties;

using Core.Models;
using Core.Models.Faculty.Identifiers;

public class FacultyMembershipConfiguration : IEntityTypeConfiguration<FacultyMembership>
{
    public void Configure(EntityTypeBuilder<FacultyMembership> builder)
    {
        builder.ToTable("Faculties_Memberships");

        builder
            .HasKey(member => member.Id);

        builder
            .Property(member => member.Id)
            .HasConversion(
                id => id.Value,
                value => FacultyMembershipId.FromValue(value));

        builder
            .HasOne<Faculty>()
            .WithMany(faculty => faculty.Members)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Staff>()
            .WithMany(staff => staff.Faculties)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
