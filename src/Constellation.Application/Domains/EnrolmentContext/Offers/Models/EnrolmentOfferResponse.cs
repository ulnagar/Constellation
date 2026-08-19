namespace Constellation.Application.Domains.EnrolmentContext.Offers.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Core.ValueObjects;
using Core.Models.EnrolmentContext.Application.Identifiers;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record EnrolmentOfferSummaryResponse(
    OfferId Id,
    Name StudentName,
    Gender StudentGender,
    string ApplicationReference,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Grade Grade,
    OfferStatus Status,
    ResponseStatus Response);
