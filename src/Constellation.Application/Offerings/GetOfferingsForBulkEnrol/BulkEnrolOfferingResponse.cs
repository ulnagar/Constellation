namespace Constellation.Application.Offerings.GetOfferingsForBulkEnrol;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using System;

public sealed record BulkEnrolOfferingResponse(
    OfferingId OfferingId,
    OfferingName Name,
    Guid FacultyId,
    string FacultyName);
