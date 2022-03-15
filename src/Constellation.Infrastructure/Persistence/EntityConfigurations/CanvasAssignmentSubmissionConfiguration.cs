using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class CanvasAssignmentSubmissionConfiguration : IEntityTypeConfiguration<CanvasAssignmentSubmission>
    {
        public void Configure(EntityTypeBuilder<CanvasAssignmentSubmission> builder)
        {
            builder.HasKey(submission => submission.Id);
            
            builder.Property(submission => submission.Id).ValueGeneratedOnAdd();
            
            builder.HasOne(submission => submission.Assignment)
                .WithMany()
                .HasForeignKey(submission => submission.AssignmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(submission => submission.Student)
                .WithMany()
                .HasForeignKey(submission => submission.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
