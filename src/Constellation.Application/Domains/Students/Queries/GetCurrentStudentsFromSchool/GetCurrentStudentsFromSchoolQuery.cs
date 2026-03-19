namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentsFromSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<List<StudentResponse>>;