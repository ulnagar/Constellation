namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assignments;

using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CanvasAssignmentConfiguration : IEntityTypeConfiguration<CanvasAssignment>
{
    public void Configure(EntityTypeBuilder<CanvasAssignment> builder)
    {
        builder.ToTable("Assignments_Assignments");

        builder
            .HasKey(assignment => assignment.Id);

        builder
            .Property(assignment => assignment.Id)
            .HasConversion(
                id => id.Value,
                value => AssignmentId.FromValue(value));

        builder
            .HasMany(assignment => assignment.Submissions)
            .WithOne()
            .HasForeignKey(submission => submission.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(assignment => assignment.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
