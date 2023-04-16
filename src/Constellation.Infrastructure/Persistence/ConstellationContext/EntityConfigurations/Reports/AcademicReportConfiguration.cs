namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Reports;

using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class AcademicReportConfiguration : IEntityTypeConfiguration<AcademicReport>
{
    public void Configure(EntityTypeBuilder<AcademicReport> builder)
    {
        builder.ToTable("Reports_AcademicReports");

        builder
            .HasKey(report => report.Id);

        builder
            .Property(report => report.Id)
            .HasConversion(
                id => id.Value,
                value => AcademicReportId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(report => report.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
