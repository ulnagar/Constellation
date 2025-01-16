namespace Constellation.Application.Offerings.GetSessionListForOffering;

using Core.Models.Offerings.Identifiers;
using Core.Models.Timetables.Identifiers;

public sealed record SessionListResponse(
    OfferingId OfferingId,
    SessionId SessionId,
    PeriodId PeriodId);
