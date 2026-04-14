namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Converters;
using Core.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentSubmissionConfiguration : IEntityTypeConfiguration<AssessmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssessmentSubmission> builder)
    {
        builder.ToTable("Submissions", "Assessments");

        builder
            .HasKey(submission => submission.Id);

        builder
            .Property(submission => submission.SubmittedByEmail)
            .HasConversion<EmailAddressConverter>();
    }
}