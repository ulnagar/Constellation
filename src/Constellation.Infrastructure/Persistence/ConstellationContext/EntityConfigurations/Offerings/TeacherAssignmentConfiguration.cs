namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Offerings;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TeacherAssignmentConfiguration
    : IEntityTypeConfiguration<TeacherAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherAssignment> builder)
    {
        builder.ToTable("Offerings_Teachers");

        builder
            .HasKey(assignment => assignment.Id);

        builder
            .Property(assignment => assignment.Id)
            .HasConversion(
                id => id.Value,
                value => AssignmentId.FromValue(value));

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(assignment => assignment.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(assignment => assignment.Offering)
            .WithMany(offering => offering.Teachers)
            .HasForeignKey(assignment => assignment.OfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(assignment => assignment.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .Property(assignment => assignment.Type)
            .HasConversion(
                entry => entry.Value,
                value => AssignmentType.FromValue(value));
    }
}
