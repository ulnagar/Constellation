namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assignments;

using Constellation.Core.Models;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;
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
            .Property(submission => submission.Id)
            .HasConversion(
                id => id.Value,
                value => AssignmentSubmissionId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(submission => submission.StudentId)
            .IsRequired();
    }
}
