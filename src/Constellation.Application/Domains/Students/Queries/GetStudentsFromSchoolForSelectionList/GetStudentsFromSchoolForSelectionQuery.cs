namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetStudentsFromSchoolForSelectionQuery(
    SchoolCode SchoolCode)
    : IQuery<List<StudentSelectionResponse>>;