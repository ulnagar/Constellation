namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SchoolContacts;

using Constellation.Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SchoolContactConfiguration : IEntityTypeConfiguration<SchoolContact>
{
    public void Configure(EntityTypeBuilder<SchoolContact> builder)
    {
        builder.ToTable("SchoolContacts_Contacts");

        builder
            .HasKey(contact => contact.Id);

        builder
            .Property(contact => contact.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolContactId.FromValue(value));

        builder
            .Property(contact => contact.PhoneNumber)
            .HasMaxLength(10);

        builder
            .HasMany(contact => contact.Assignments)
            .WithOne()
            .HasForeignKey(role => role.SchoolContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(contact => contact.Assignments)
            .AutoInclude();
    }
}