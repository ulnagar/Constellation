namespace Constellation.Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplications;

using Constellation.Core.Enums;
using Constellation.Core.Models.Common.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.StudentOnboarding.Enums;
using Constellation.Core.Models.StudentOnboarding.Identifiers;
using Constellation.Core.Models.StudentOnboarding.Policy;
using Constellation.Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using System;
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