namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentsFromSchoolForSelectionQuery(
    string SchoolCode)
    : IQuery<List<StudentSelectionResponse>>;