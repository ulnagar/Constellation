namespace Constellation.Application.Domains.EnrolmentContext.Applications.Models;

using Core.Enums;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.Models.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record EnrolmentApplicationResponse(
    ApplicationId Id,
    EnrolmentPeriodId PeriodId,
    string PeriodName,
    string PeriodYear,
    EnrolmentStudentId StudentId,
    StudentReferenceNumber? StudentReferenceNumber,
    Name StudentName,
    Gender StudentGender,
    DateOnly? DateOfBirth,
    EmailAddress? StudentEmailAddress,
    List<EnrolmentApplicationResponse.Parent> Parents,
    string ApplicationReference,
    SchoolCode? CurrentSchoolCode,
    string CurrentSchool,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Program Program,
    Grade Grade,
    ApplicationStatus Status,
    IReadOnlyList<CourseSelection> SelectedCourses)
{
    public sealed record Parent(
        EnrolmentParentId Id,
        Name Name,
        EmailAddress? Email,
        PhoneNumber? PhoneNumber,
        MailingAddress? MailingAddress);
}
