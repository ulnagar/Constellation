namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.OwnsOne(contact => contact.Gender);

        builder.HasMany(contact => contact.Roles).WithOne(role => role.Contact).OnDelete(DeleteBehavior.NoAction);
    }
}
