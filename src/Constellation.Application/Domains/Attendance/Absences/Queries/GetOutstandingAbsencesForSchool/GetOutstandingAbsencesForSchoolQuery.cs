namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetOutstandingAbsencesForSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<List<OutstandingAbsencesForSchoolResponse>>;