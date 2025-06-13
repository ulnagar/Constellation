namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StaffMembers;

using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.Students.Enums;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.ToTable("Members", "Staff");

        builder
            .HasKey(member => member.Id);

        builder
            .Property(member => member.Id)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));

        builder
            .Property(member => member.EmployeeId)
            .IsRequired(false)
            .HasConversion(
                id => id.Number,
                value => EmployeeId.FromValue(value));

        builder
            .HasIndex(member => member.EmployeeId)
            .IsUnique();

        builder
            .ComplexProperty(member => member.Name)
            .IsRequired();

        builder
            .ComplexProperty(member => member.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(member => member.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(member => member.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(member => member.EmailAddress)
            .HasConversion(
                email => email.Email,
                value => EmailAddress.FromValue(value));

        builder
            .Property(member => member.Gender)
            .HasConversion(
                gender => gender.Value,
                value => Gender.FromValue(value));

        builder
            .HasMany(member => member.SchoolAssignments)
            .WithOne()
            .HasForeignKey(assignment => assignment.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(member => member.SchoolAssignments)
            .AutoInclude();

        builder
            .HasMany(member => member.SystemLinks)
            .WithOne()
            .HasForeignKey(link => link.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(member => member.SystemLinks)
            .AutoInclude();
    }
}