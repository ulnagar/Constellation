namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assignments;

using Constellation.Core.Models;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CanvasAssignmentSubmissionConfiguration : IEntityTypeConfiguration<CanvasAssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<CanvasAssignmentSubmission> builder)
    {
        builder.ToTable("Assignments_Submissions");

        builder
            .HasKey(submission => submission.Id);

        builder
            .Property(submission => submission.Id)
            .HasConversion(
                id => id.Value,
                value => AssignmentSubmissionId.FromValue(value));

        // If this relationship is defined from both sides, and extra column is added to the database
        //builder
        //    .HasOne<CanvasAssignment>()
        //    .WithMany()
        //    .HasForeignKey(submission => submission.AssignmentId)
        //    .IsRequired()
        //    .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(submission => submission.StudentId)
            .IsRequired();
    }
}
