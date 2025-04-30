namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForFamily;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAbsencesForFamilyQuery(
    string ParentEmail)
    : IQuery<List<AbsenceForFamilyResponse>>;
