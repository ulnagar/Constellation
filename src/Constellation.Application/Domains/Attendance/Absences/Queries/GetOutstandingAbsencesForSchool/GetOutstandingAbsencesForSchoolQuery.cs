namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetOutstandingAbsencesForSchoolQuery(
    string SchoolCode)
    : IQuery<List<OutstandingAbsencesForSchoolResponse>>;