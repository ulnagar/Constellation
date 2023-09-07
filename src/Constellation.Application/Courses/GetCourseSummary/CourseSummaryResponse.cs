namespace Constellation.Application.Courses.GetCourseSummary;

using Constellation.Core.Enums;
using System;

public sealed record CourseSummaryResponse(
    int CourseId,
    string Name,
    string Code,
    Grade Grade,
    Guid FacultyId,
    string FacultyName);