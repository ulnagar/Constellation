namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Converters;
using Core.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentDownloadEventConfiguration : IEntityTypeConfiguration<AssessmentDownloadEvent>
{
    public void Configure(EntityTypeBuilder<AssessmentDownloadEvent> builder)
    {
        builder.ToTable("DownloadEvents", "Assessments");

        builder
            .HasKey(downloadEvent => new { downloadEvent.DownloadId, downloadEvent.UserId, downloadEvent.DownloadedAt });

        builder
            .Property(downloadEvent => downloadEvent.DownloadedByEmail)
            .HasConversion<EmailAddressConverter>();
    }
}