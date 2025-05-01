namespace Constellation.Application.Domains.Courses.Commands.CreateCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Core.Models.Faculties.Identifiers;

public sealed record CreateCourseCommand(
    string Name,
    string Code,
    Grade Grade,
    FacultyId FacultyId,
    decimal FTEValue,
    double Target)
    : ICommand;