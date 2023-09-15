namespace Constellation.Application.Courses.UpdateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record UpdateCourseCommand(
    CourseId CourseId,
    string Name,
    string Code,
    Grade Grade,
    Guid FacultyId,
    decimal FTEValue)
    : ICommand;
