namespace Constellation.Core.Models.Reports;

using Constellation.Core.Models.Students.Identifiers;
using DomainEvents;
using Identifiers;
using Primitives;

public sealed class AcademicReport : AggregateRoot
{
    private AcademicReport(
        AcademicReportId id, 
        StudentId studentId, 
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
    public StudentId StudentId { get; private set; }
    public string PublishId { get; private set; }
    public string Year { get; private set; }
    public string ReportingPeriod { get; private set; }

    public static AcademicReport Create(
        AcademicReportId id,
        StudentId studentId,
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
