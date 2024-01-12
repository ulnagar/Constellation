namespace Constellation.Application.Students.GetCurrentStudentsFromSchool;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using System.Collections.Generic;

public sealed record GetCurrentStudentsFromSchoolQuery(
    string SchoolCode)
    : IQuery<List<StudentDto>>;