namespace Constellation.Application.Domains.Students.Queries.GetFilteredStudents;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFilteredStudentsQuery(
    StudentFilter Filter)
    : IQuery<List<FilteredStudentResponse>>;
