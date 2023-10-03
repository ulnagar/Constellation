namespace Constellation.Application.Offerings.GetSessionListForOffering;

using Core.Models.Offerings.Identifiers;

public sealed record SessionListResponse(
    OfferingId OfferingId,
    SessionId SessionId,
    int PeriodId);
