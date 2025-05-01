namespace Constellation.Application.Domains.Courses.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties.Identifiers;

public sealed record CourseSelectListItemResponse(
    CourseId Id,
    string Name,
    Grade Grade,
    FacultyId FacultyId,
    string FacultyName,
    string DisplayName);