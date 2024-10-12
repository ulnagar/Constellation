namespace Constellation.Application.Timetables.GetStudentTimetableData;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Students.Identifiers;

public sealed record GetStudentTimetableDataQuery(
    StudentId StudentId)
    : IQuery<StudentTimetableDataDto>;