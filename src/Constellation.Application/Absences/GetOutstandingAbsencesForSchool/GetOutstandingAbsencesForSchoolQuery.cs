namespace Constellation.Application.Absences.GetOutstandingAbsencesForSchool;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetOutstandingAbsencesForSchoolQuery(
    string SchoolCode)
    : IQuery<List<OutstandingAbsencesForSchoolResponse>>;