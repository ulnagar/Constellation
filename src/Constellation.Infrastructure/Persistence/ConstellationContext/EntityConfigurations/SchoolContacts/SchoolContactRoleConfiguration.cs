namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.SchoolContacts;

using Constellation.Core.Models.SchoolContacts;
using Core.Models;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SchoolContactRoleConfiguration : IEntityTypeConfiguration<SchoolContactRole>
{
    public void Configure(EntityTypeBuilder<SchoolContactRole> builder)
    {
        builder.ToTable("SchoolContacts_Roles");

        builder
            .HasKey(role => role.Id);

        builder
            .Property(role => role.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolContactRoleId.FromValue(value));

        builder
            .Property(role => role.SchoolContactId)
            .HasConversion(
                id => id.Value,
                value => SchoolContactId.FromValue(value));

        builder
            .Property(role => role.Role)
            .HasConversion(
                role => role.Value,
                value => Position.FromValue(value));

        builder.HasOne<School>()
            .WithMany(s => s.StaffAssignments)
            .OnDelete(DeleteBehavior.Cascade);
    }
}