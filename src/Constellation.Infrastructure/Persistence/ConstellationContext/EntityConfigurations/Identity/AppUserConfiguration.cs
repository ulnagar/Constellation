namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Identity;

using Application.Models.Identity;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder
            .Property(user => user.SchoolContactId)
            .HasConversion(
                id => id.Value,
                value => SchoolContactId.FromValue(value));

        builder
            .Property(user => user.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));

        builder
            .Property(user => user.StaffId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));
    }
}
