namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;

using Core.Enums;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.Models.Identifiers;
using Core.Models.Students.Enums;
using Core.ValueObjects;

public sealed record EnrolmentApplicationSummary(
    ApplicationId Id,
    Name StudentName,
    Gender StudentGender,
    string ApplicationReference,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Program Program,
    Grade Grade,
    ApplicationStatus Status);