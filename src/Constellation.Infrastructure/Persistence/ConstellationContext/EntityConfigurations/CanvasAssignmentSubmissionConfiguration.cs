namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Constellation.Core.Models.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CanvasAssignmentSubmissionConfiguration : IEntityTypeConfiguration<CanvasAssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<CanvasAssignmentSubmission> builder)
    {
        builder.HasKey(submission => submission.Id);

        builder.Property(submission => submission.Id).ValueGeneratedOnAdd();

        builder.HasOne<CanvasAssignment>()
            .WithMany()
            .HasForeignKey(submission => submission.AssignmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(submission => submission.StudentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
