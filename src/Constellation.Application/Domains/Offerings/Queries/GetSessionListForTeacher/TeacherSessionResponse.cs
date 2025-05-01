namespace Constellation.Application.Domains.Offerings.Queries.GetSessionListForTeacher;

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