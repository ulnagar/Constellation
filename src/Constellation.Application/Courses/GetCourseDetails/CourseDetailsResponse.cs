namespace Constellation.Application.Courses.GetCourseDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties.Identifiers;
using System;
using System.Collections.Generic;

public sealed record CourseDetailsResponse(
    CourseId Id,
    string Name,
    string Code,
    Grade Grade,
    CourseDetailsResponse.Faculty CourseFaculty,
    List<CourseDetailsResponse.Offering> Offerings,
    decimal FTEValue,
    decimal FTETotal)
{
    public sealed record Faculty(
        FacultyId FacultyId,
        string Name,
        string Colour);

    public sealed record Teacher(
        string StaffId,
        string Name);

    public sealed record Offering(
        OfferingId Id,
        OfferingName Name,
        List<Teacher> Teachers,
        DateOnly EndDate,
        bool IsCurrent,
        bool IsFuture);
}
