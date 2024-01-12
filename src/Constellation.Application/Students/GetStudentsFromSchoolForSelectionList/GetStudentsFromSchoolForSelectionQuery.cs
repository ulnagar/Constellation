namespace Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentsFromSchoolForSelectionQuery(
    string SchoolCode)
    : IQuery<List<StudentSelectionResponse>>;