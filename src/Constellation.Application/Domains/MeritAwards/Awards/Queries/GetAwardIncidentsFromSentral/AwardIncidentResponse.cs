namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardIncidentsFromSentral;

using System;

public sealed record AwardIncidentResponse(
    DateTime AwardedAt,
    DateOnly DateIssued,
    string IncidentId,
    string TeacherName,
    string IssueReason);