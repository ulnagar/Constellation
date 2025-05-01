namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingsForBulkEnrol;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Faculties.Identifiers;

public sealed record BulkEnrolOfferingResponse(
    OfferingId OfferingId,
    OfferingName Name,
    FacultyId FacultyId,
    string FacultyName);
