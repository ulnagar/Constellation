namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContactRoleConfiguration : IEntityTypeConfiguration<ContactRole>
{
    public void Configure(EntityTypeBuilder<ContactRole> builder)
    {
        builder.HasOne(role => role.Contact).WithMany(contact => contact.Roles).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(role => role.School).WithMany(school => school.Contacts).OnDelete(DeleteBehavior.NoAction);

        builder.OwnsOne(role => role.Position);
    }
}
