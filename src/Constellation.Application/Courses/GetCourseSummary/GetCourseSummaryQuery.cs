﻿namespace Constellation.Application.Courses.GetCourseSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Courses.Models;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record GetCourseSummaryQuery(
    CourseId CourseId)
    : IQuery<CourseSummaryResponse>;