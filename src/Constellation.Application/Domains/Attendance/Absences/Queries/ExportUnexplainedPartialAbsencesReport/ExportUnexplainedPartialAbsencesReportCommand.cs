namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportUnexplainedPartialAbsencesReport;

using Abstractions.Messaging;
using DTOs;

public sealed record ExportUnexplainedPartialAbsencesReportCommand
    : ICommand<FileDto>;