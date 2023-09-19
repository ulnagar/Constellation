namespace Constellation.Application.Offerings.GetSessionListForTeacher;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;

public sealed record TeacherSessionResponse(
    OfferingId OfferingId,
    OfferingName OfferingName,
    SessionId SessionId,
    int PeriodId,
    string PeriodName,
    decimal Duration);