namespace Constellation.Application.Domains.Students.Queries.GetStudents;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Students.Models;
using System.Collections.Generic;

public sealed record GetStudentsQuery()
    : IQuery<List<StudentResponse>>;