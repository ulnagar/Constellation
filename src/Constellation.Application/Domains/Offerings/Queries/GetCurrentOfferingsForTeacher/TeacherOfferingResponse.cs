namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;

public sealed record TeacherOfferingResponse(
    OfferingId OfferingId,
    OfferingName OfferingName,
    string CourseName,
    AssignmentType AssignmentType);