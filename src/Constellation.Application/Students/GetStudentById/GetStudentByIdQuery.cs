namespace Constellation.Application.Students.GetStudentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Students.Models;

public sealed record GetStudentByIdQuery(
    string StudentId)
    : IQuery<StudentResponse>;