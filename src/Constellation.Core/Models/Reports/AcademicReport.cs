namespace Constellation.Core.Models.Reports;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;

public sealed class AcademicReport : AggregateRoot
{
    private AcademicReport(
        AcademicReportId id, 
        string studentId, 
        string publishId, 
        string year, 
        string reportingPeriod)
    {
        Id = id;
        StudentId = studentId;
        PublishId = publishId;
        Year = year;
        ReportingPeriod = reportingPeriod;
    }

    public AcademicReportId Id { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public string PublishId { get; private set; } = string.Empty;
    public string Year { get; private set; } = string.Empty;
    public string ReportingPeriod { get; private set; } = string.Empty;

    public static AcademicReport Create(
        AcademicReportId id,
        string studentId,
        string publishId,
        string year,
        string reportingPeriod)
    {
        AcademicReport report = new(
            id,
            studentId,
            publishId,
            year,
            reportingPeriod);

        report.RaiseDomainEvent(new AcademicReportCreatedDomainEvent(new DomainEventId(), id));

        return report;
    }

    public void Update(string publishId) => PublishId = publishId;
}
