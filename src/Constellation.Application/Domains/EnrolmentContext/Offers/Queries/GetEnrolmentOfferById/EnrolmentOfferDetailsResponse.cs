namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetEnrolmentOfferById;

using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.ValueObjects;

public sealed record EnrolmentOfferDetailsResponse(
    OfferId Id,
    ApplicationId ApplicationId,
    EnrolmentPeriodId PeriodId,
    string PeriodName,
    EnrolmentStudentId StudentId,
    StudentReferenceNumber? StudentReferenceNumber,
    Name StudentName,
    Gender StudentGender,
    DateOnly? DateOfBirth,
    EmailAddress? StudentEmailAddress,
    List<EnrolmentOfferDetailsResponse.Parent> Parents,
    string ApplicationReference,
    SchoolCode? CurrentSchoolCode,
    string CurrentSchool,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Program Program,
    Grade Grade,
    List<EnrolmentCourse> SelectedCourses,
    OfferStatus Status,
    DateTimeOffset? OfferedAt,
    DateTimeOffset? RespondBy,
    DateTimeOffset? RespondedAt,
    ResponseStatus Response,
    bool HasCourtOrders,
    bool HasHealthConcerns,
    bool RequestedLaptop,
    List<EnrolmentOfferDetailsResponse.Note> Notes)
{
    public sealed record Parent(
        EnrolmentParentId Id,
        Name Name,
        EmailAddress Email,
        PhoneNumber PhoneNumber,
        MailingAddress? MailingAddress);

    public sealed record Note(
        DateTimeOffset Timestamp,
        string CreatedBy,
        string Message);
}