﻿namespace Constellation.Application.Domains.Assignments.Queries.GetRubricAssignmentsFromCourse;

using Abstractions.Messaging;
using Core.Models.Subjects.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetRubricAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;