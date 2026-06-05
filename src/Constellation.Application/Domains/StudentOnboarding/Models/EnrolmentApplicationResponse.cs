namespace Constellation.Application.Domains.StudentOnboarding.Models;

using Core.Enums;
using Core.Models.Common.Enums;
using Core.Models.Identifiers;
using Core.Models.StudentOnboarding.Enums;
using Core.Models.StudentOnboarding.Identifiers;
using Core.Models.StudentOnboarding.Policy;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using ApplicationId = Core.Models.StudentOnboarding.Identifiers.ApplicationId;

public sealed record EnrolmentApplicationResponse(
    ApplicationId ApplicationId,
    ApplicantId ApplicantId,
    StudentReferenceNumber? StudentReferenceNumber,
    Name Name,
    EmailAddress? EmailAddress,
    Gender? Gender,
    IndigenousStatus IndigenousStatus,
    Program Program,
    string Year,
    Grade Grade,
    SchoolCode? SchoolCode,
    string? SchoolName,
    ApplicationState State,
    DateOnly Deadline);