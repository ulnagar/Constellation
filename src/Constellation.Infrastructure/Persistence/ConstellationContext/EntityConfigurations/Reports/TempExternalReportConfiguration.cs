namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Reports;

using Core.Models.Reports;
using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class TempExternalReportConfiguration : IEntityTypeConfiguration<TempExternalReport>
{
    public void Configure(EntityTypeBuilder<TempExternalReport> builder)
    {
        builder.ToTable("TempReports", "Reports");

        builder
            .HasKey(report => report.Id);

        builder
            .Property(report => report.Id)
            .HasConversion(
                id => id.Value,
                value => ExternalReportId.FromValue(value));

        builder
            .Property(report => report.StudentId)
            .HasConversion(
                type => type.Value,
                value => StudentId.FromValue(value));

        builder
            .Property(report => report.Type)
            .HasConversion(
                type => type.Value,
                value => ReportType.FromValue(value));
    }
}