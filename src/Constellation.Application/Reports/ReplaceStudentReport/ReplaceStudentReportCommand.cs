namespace Constellation.Application.Reports.ReplaceStudentReport;

using Constellation.Application.Abstractions.Messaging;

public sealed record ReplaceStudentReportCommand(
    string OldPublishId,
    string NewPublishId,
    byte[] FileData)
    : ICommand;