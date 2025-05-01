namespace Constellation.Application.Domains.Students.Queries.GetStudentById;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Models;

public sealed record GetStudentByIdQuery(
    StudentId StudentId)
    : IQuery<StudentResponse>;