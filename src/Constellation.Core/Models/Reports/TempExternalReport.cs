namespace Constellation.Core.Models.Reports;

using Enums;
using Identifiers;
using Primitives;
using Students.Identifiers;
using System;

public sealed class TempExternalReport : AggregateRoot
{
    private TempExternalReport()
    {
        Id = new();
    }

    private TempExternalReport(
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

    public static TempExternalReport Create() => new();

    public static TempExternalReport Create(
        StudentId studentId,
        ReportType type,
        DateOnly issuedDate)
    {
        return new(
            studentId,
            type,
            issuedDate);
    }

    public void UpdateStudentId(StudentId studentId) 
        => StudentId = studentId;

    public void UpdateReportType(ReportType reportType)
        => Type = reportType;

    public void UpdateIssuedDate(DateOnly issuedDate)
        => IssuedDate = issuedDate;
}