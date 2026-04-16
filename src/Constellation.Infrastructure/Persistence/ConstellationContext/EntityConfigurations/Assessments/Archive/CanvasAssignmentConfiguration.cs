namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments.Archive;

using Constellation.Core.Models.Assessments.Archive;
using Constellation.Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CanvasAssignmentConfiguration : IEntityTypeConfiguration<CanvasAssignment>
{
    public void Configure(EntityTypeBuilder<CanvasAssignment> builder)
    {
        builder.ToTable("Assignments_Assignments");

        builder
            .HasKey(assignment => assignment.Id);

        builder
            .Navigation(assignment => assignment.Submissions)
            .AutoInclude();

        builder
            .HasMany(assignment => assignment.Submissions)
            .WithOne()
            .HasForeignKey(submission => submission.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Course>()
            .WithMany()
            .HasForeignKey(assignment => assignment.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
