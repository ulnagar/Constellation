namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Offerings.Enums;

public sealed record TeacherOfferingResponse(
    OfferingId OfferingId,
    OfferingName OfferingName,
    string Year,
    string CourseName,
    AssignmentType AssignmentType);