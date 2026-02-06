namespace Constellation.Presentation.Staff.Areas.Staff.Models;

using Core.Enums;
using Core.Models.Subjects.Identifiers;

public sealed record CourseRecord(
    CourseId Id,
    string Name,
    Grade Grade);