﻿namespace Constellation.Application.Domains.Courses.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties.Identifiers;
using System.Collections.Generic;

public sealed record CourseSummaryResponse(
    CourseId CourseId,
    string Name,
    string Code,
    Grade Grade,
    CourseSummaryResponse.Faculty CourseFaculty,
    decimal FTEValue,
    double TargetPerCycle,
    List<CourseSummaryResponse.Offering> Offerings)
{
    public sealed record Faculty(
        FacultyId FacultyId,
        string Name,
        string Colour);

    public sealed record Offering(
        OfferingId OfferingId,
        OfferingName Name,
        bool IsCurrent);
}