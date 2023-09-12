namespace Constellation.Application.Courses.GetCourseSummary;

using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record CourseSummaryResponse(
    CourseId CourseId,
    string Name,
    string Code,
    Grade Grade,
    Guid FacultyId,
    string FacultyName);