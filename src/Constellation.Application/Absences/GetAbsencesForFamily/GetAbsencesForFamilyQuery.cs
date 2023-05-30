namespace Constellation.Application.Absences.GetAbsencesForFamily;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAbsencesForFamilyQuery(
    string ParentEmail)
    : IQuery<List<AbsenceForFamilyResponse>>;
