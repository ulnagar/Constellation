namespace Constellation.Application.Timetables.GetStudentTimetableExport;

using Abstractions.Messaging;
using DTOs;

public sealed record GetStudentTimetableExportQuery(
    string StudentId)
    : IQuery<FileDto>;