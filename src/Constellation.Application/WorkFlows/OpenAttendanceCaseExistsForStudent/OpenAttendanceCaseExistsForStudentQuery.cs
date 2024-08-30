namespace Constellation.Application.WorkFlows.OpenAttendanceCaseExistsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record OpenAttendanceCaseExistsForStudentQuery(
    StudentId StudentId)
    : IQuery<bool>;