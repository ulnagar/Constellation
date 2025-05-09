﻿namespace Constellation.Application.Domains.Courses.Commands.UpdateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties.Identifiers;

public sealed record UpdateCourseCommand(
    CourseId CourseId,
    string Name,
    string Code,
    Grade Grade,
    FacultyId FacultyId,
    decimal FTEValue,
    double Target)
    : ICommand;
