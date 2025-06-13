namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StaffMembers;

using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SchoolAssignmentConfiguration : IEntityTypeConfiguration<SchoolAssignment>
{
    public void Configure(EntityTypeBuilder<SchoolAssignment> builder)
    {
        builder.ToTable("SchoolAssignments", "Staff");

        builder
            .HasKey(assignment => assignment.Id);

        builder
            .Property(assignment => assignment.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolAssignmentId.FromValue(value));

        builder
            .HasOne<StaffMember>()
            .WithMany(member => member.SchoolAssignments)
            .HasForeignKey(assignment => assignment.StaffId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<School>()
            .WithMany()
            .HasForeignKey(assignment => assignment.SchoolCode)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}