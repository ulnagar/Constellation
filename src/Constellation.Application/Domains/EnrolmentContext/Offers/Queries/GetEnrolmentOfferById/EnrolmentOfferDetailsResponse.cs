namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetEnrolmentOfferById;

using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Models.EnrolmentContext.Application.Identifiers;
using Core.ValueObjects;

public sealed record EnrolmentOfferDetailsResponse(
    OfferId Id,
    ApplicationId ApplicationId,
    EnrolmentPeriodId PeriodId,
    string PeriodName,
    StudentReferenceNumber? StudentReferenceNumber,
    Name StudentName,
    Gender StudentGender,
    Name? ParentName,
    EmailAddress? ParentEmailAddress,
    PhoneNumber? ParentPhoneNumber,
    string ApplicationReference,
    SchoolCode? CurrentSchoolCode,
    string CurrentSchool,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Program Program,
    Grade Grade,
    OfferStatus Status,
    DateTimeOffset? OfferedAt,
    DateTimeOffset? RespondBy,
    DateTimeOffset? RespondedAt,
    bool HasCourtOrders,
    bool HasHealthConcerns,
    bool RequestedLaptop);