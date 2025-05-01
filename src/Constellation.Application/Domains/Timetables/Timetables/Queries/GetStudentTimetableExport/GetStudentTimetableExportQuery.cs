namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableExport;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using DTOs;

public sealed record GetStudentTimetableExportQuery(
    StudentId StudentId)
    : IQuery<FileDto>;