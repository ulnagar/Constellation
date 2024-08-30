namespace Constellation.Application.Students.GetCurrentStudentsFromSchool;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentsFromSchoolQuery(
    string SchoolCode)
    : IQuery<List<StudentResponse>>;