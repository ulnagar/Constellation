namespace Constellation.Application.Offerings.GetSessionListForTeacher;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Timetables.Identifiers;

public sealed record TeacherSessionResponse(
    OfferingId OfferingId,
    OfferingName OfferingName,
    SessionId SessionId,
    PeriodId PeriodId,
    string PeriodSortOrder,
    string PeriodName,
    decimal Duration);