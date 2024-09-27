namespace Constellation.Application.Students.GetSchoolEnrolmentHistoryForStudent;

using Core.Enums;
using Core.Models.Students.Identifiers;
using System;

public sealed record SchoolEnrolmentResponse(
    SchoolEnrolmentId Id,
    string SchoolCode,
    string SchoolName,
    Grade Grade,
    int Year,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsDeleted);