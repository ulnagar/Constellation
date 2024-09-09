namespace Constellation.Application.Students.GetSchoolEnrolmentHistoryForStudent;

using Core.Enums;
using System;

public sealed record SchoolEnrolmentResponse(
    string SchoolCode,
    string SchoolName,
    Grade Grade,
    int Year,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsDeleted);