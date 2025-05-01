namespace Constellation.Application.Domains.LinkedSystems.Canvas.Models;

using Core.Models.Canvas.Models;

public sealed record CourseListEntry(
    string Name,
    CanvasCourseCode CourseCode);
