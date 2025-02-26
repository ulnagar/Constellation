namespace Constellation.Application.Attendance.GenerateCustomReportForPeriod;

using Core.Enums;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record ExportRecord(
    StudentReferenceNumber StudentReferenceNumber,
    Name Name,
    Grade Grade,
    string SchoolName,
    IndigenousStatus IndigenousStatus,
    decimal Value);