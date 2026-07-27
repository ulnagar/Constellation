namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.ExportOfferList;

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

public sealed record EnrolmentOfferExport(
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
    DateTime? OfferedAt,
    DateTime? RespondBy,
    DateTime? ReminderSentAt,
    DateTime? RespondedAt);
