namespace Constellation.Application.Domains.StudentReports.Commands.ReplaceStudentReport;

using Abstractions.Messaging;

public sealed record ReplaceStudentReportCommand(
    string OldPublishId,
    string NewPublishId,
    byte[] FileData)
    : ICommand;