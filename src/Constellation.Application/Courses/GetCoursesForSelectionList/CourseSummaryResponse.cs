namespace Constellation.Application.Courses.GetCoursesForSelectionList;

using Constellation.Core.Enums;
using System;

public sealed record CourseSummaryResponse(
    int Id,
    string Name,
    Grade Grade,
    Guid FacultyId,
    string FacultyName,
    string DisplayName);