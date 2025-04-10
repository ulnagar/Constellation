﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Reports;

using Constellation.Core.Models.Reports;
using Constellation.Core.Models.Reports.Identifiers;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class AcademicReportConfiguration : IEntityTypeConfiguration<AcademicReport>
{
    public void Configure(EntityTypeBuilder<AcademicReport> builder)
    {
        builder.ToTable("AcademicReports", "Reports");

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