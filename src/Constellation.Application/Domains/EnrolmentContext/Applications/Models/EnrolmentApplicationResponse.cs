namespace Constellation.Application.Domains.EnrolmentContext.Applications.Models;

using Core.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record EnrolmentApplicationResponse(
    ApplicationId Id,
    EnrolmentPeriodId PeriodId,
    StudentReferenceNumber? StudentReferenceNumber,
    Name StudentName,
    Gender StudentGender,
    DateOnly? DateOfBirth,
    EmailAddress? StudentEmailAddress,
    Name? ParentName,
    EmailAddress? ParentEmailAddress,
    PhoneNumber? ParentPhoneNumber,
    MailingAddress? MailingAddress,
    string ApplicationReference,
    SchoolCode? CurrentSchoolCode,
    string CurrentSchool,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Program Program,
    Grade Grade);
