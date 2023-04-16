namespace Constellation.Application.Reports.CreateNewStudentReport;

using Constellation.Application.Abstractions.Messaging;

public sealed record CreateNewStudentReportCommand(
    string StudentId,
    string PublishId,
    string Year,
    string ReportingPeriod,
    byte[] FileData,
    string FileName)
    : ICommand;