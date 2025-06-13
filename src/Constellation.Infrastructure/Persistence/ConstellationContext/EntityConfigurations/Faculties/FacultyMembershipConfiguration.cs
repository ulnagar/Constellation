namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Faculties;

using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class FacultyMembershipConfiguration : IEntityTypeConfiguration<FacultyMembership>
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
            .HasOne<StaffMember>()
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(member => member.Role)
            .HasConversion(
                entry => entry.Value,
                value => FacultyMembershipRole.FromValue(value));
    }
}
