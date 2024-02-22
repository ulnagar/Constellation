namespace Constellation.Application.Absences.ExportUnexplainedPartialAbsencesReport;

using Abstractions.Messaging;
using DTOs;

public sealed record ExportUnexplainedPartialAbsencesReportCommand
    : ICommand<FileDto>;