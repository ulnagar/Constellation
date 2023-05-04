namespace Constellation.Application.Awards.GetSummaryForStudent;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record StudentAwardSummaryResponse(
    int Astras,
    int Stellars,
    int Galaxies,
    int Universals,
    List<StudentAwardSummaryResponse.StudentAwardResponse> RecentAwards)
{
    public sealed record StudentAwardResponse(
        StudentAwardId AwardId,
        string Type,
        DateTime AwardedOn,
        string AwardedBy,
        string AwardedFor,
        string IncidentId,
        bool HasCertificate);
}
