namespace Constellation.Application.WorkFlows.OpenAttendanceCaseExistsForStudent;

using Abstractions.Messaging;

public sealed record OpenAttendanceCaseExistsForStudentQuery(
    string StudentId)
    : IQuery<bool>;