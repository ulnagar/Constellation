namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentDownloadConfiguration : IEntityTypeConfiguration<AssessmentDownload>
{
    public void Configure(EntityTypeBuilder<AssessmentDownload> builder)
    {
        builder.ToTable("Downloads", "Assessments");

        builder
            .HasKey(download => download.Id);

        builder
            .HasMany(download => download.DownloadEvents)
            .WithOne()
            .HasForeignKey(downloadEvent => downloadEvent.DownloadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(download => download.DownloadEvents)
            .HasField("_downloadEvents")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}