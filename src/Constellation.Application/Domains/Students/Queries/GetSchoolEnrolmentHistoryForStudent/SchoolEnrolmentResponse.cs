namespace Constellation.Application.Domains.Students.Queries.GetSchoolEnrolmentHistoryForStudent;

using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System;

public sealed record SchoolEnrolmentResponse(
    SchoolEnrolmentId Id,
    SchoolCode SchoolCode,
    string SchoolName,
    Grade Grade,
    int Year,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsDeleted);