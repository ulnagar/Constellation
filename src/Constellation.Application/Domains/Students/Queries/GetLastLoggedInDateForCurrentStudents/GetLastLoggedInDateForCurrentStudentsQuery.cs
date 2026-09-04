namespace Constellation.Application.Domains.Students.Queries.GetLastLoggedInDateForCurrentStudents;

using Abstractions.Messaging;
using Constellation.Application.Domains.Students.Models;
using Models;
using System.Collections.Generic;

public sealed record GetLastLoggedInDateForCurrentStudentsQuery()
    : IQuery<List<StudentLoginData>>;