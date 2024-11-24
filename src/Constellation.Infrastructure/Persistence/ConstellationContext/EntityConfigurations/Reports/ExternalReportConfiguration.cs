namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Reports;

using Core.Models.Reports;
using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class ExternalReportConfiguration : IEntityTypeConfiguration<ExternalReport>
{
    public void Configure(EntityTypeBuilder<ExternalReport> builder)
    {
        builder.ToTable("ExternalReports", "Reports");

        builder
            .HasKey(report => report.Id);

        builder
            .Property(report => report.Id)
            .HasConversion(
                id => id.Value,
                value => ExternalReportId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(report => report.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(report => report.Type)
            .HasConversion(
                type => type.Value,
                value => ReportType.FromValue(value));
    }
}