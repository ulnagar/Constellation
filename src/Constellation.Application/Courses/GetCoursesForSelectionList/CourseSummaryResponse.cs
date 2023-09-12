namespace Constellation.Application.Courses.GetCoursesForSelectionList;

using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record CourseSummaryResponse(
    CourseId Id,
    string Name,
    Grade Grade,
    Guid FacultyId,
    string FacultyName,
    string DisplayName);