namespace Constellation.Application.Courses.CreateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using System;

public sealed record CreateCourseCommand(
    string Name,
    string Code,
    Grade Grade,
    Guid FacultyId,
    decimal FTEValue)
    : ICommand;