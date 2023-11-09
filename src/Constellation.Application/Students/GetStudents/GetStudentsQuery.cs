namespace Constellation.Application.Students.GetStudents;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Students.Models;
using System.Collections.Generic;

public sealed record GetStudentsQuery()
    : IQuery<List<StudentResponse>>;