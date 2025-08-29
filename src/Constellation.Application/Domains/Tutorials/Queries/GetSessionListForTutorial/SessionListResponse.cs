namespace Constellation.Application.Domains.Tutorials.Queries.GetSessionListForTutorial;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Identifiers;

public sealed record SessionListResponse(
    TutorialId OfferingId,
    TutorialSessionId SessionId,
    PeriodId PeriodId,
    StaffId StaffId);
