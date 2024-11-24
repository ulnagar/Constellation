namespace Constellation.Core.Models.Reports;

using Constellation.Core.Models.Reports.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Primitives;
using System;

public sealed class ExternalReport : AggregateRoot
{
    private ExternalReport(
        StudentId studentId,
        ReportType type,
        DateOnly issuedDate)
    {
        Id = new();
        StudentId = studentId;
        Type = type;
        IssuedDate = issuedDate;
    }

    public ExternalReportId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public ReportType Type { get; private set; }
    public DateOnly IssuedDate { get; private set; }

    public static ExternalReport Create(
        StudentId studentId,
        ReportType type,
        DateOnly issuedDate)
    {
        return new(
            studentId,
            type,
            issuedDate);
    }
}