namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.ScheduledReports;

using Application.ScheduledReports;
using Application.ScheduledReports.Identifiers;
using Converters;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ScheduledReportConfiguration : IEntityTypeConfiguration<ScheduledReport>
{
    public void Configure(EntityTypeBuilder<ScheduledReport> builder)
    {
        builder.ToTable("ScheduledReports", "Automation");

        builder
            .HasKey(report => report.Id);

        builder
            .Property(report => report.Id)
            .HasConversion(
                id => id.Value,
                value => ScheduledReportId.FromValue(value));

        builder
            .ComplexProperty(report => report.ForwardTo)
            .Property(recipient => recipient.Email)
            .HasColumnName(nameof(EmailRecipient.Email));

        builder
            .ComplexProperty(report => report.ForwardTo)
            .Property(recipient => recipient.Name)
            .HasColumnName(nameof(EmailRecipient.Name));

        builder
            .Property(report => report.LastResult)
            .HasConversion<ResultConverter>();

    }
}
