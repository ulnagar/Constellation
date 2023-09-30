namespace Constellation.Application.Timetables.GetStudentTimetableData;

using Abstractions.Messaging;
using Constellation.Application.DTOs;

public sealed record GetStudentTimetableDataQuery(
    string StudentId)
    : IQuery<StudentTimetableDataDto>;