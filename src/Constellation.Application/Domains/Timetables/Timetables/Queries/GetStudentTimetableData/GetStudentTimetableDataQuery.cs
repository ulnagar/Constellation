namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableData;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using DTOs;

public sealed record GetStudentTimetableDataQuery(
    StudentId StudentId)
    : IQuery<StudentTimetableDataDto>;