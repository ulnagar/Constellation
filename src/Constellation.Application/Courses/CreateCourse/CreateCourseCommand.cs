namespace Constellation.Application.Courses.CreateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Core.Models.Faculty.Identifiers;

public sealed record CreateCourseCommand(
    string Name,
    string Code,
    Grade Grade,
    FacultyId FacultyId,
    decimal FTEValue)
    : ICommand;