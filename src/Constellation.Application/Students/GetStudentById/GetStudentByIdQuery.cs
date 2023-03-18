namespace Constellation.Application.Students.GetStudentById;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetStudentByIdQuery(
    string StudentId)
    : IQuery<StudentResponse>;