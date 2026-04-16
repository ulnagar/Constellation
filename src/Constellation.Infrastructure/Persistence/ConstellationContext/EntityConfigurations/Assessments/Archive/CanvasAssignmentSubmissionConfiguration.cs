namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments.Archive;

using Constellation.Core.Models.Assessments.Archive;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CanvasAssignmentSubmissionConfiguration : IEntityTypeConfiguration<CanvasAssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<CanvasAssignmentSubmission> builder)
    {
        builder.ToTable("Assignments_Submissions");

        builder
            .HasKey(submission => submission.Id);

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(submission => submission.StudentId)
            .IsRequired();
    }
}
