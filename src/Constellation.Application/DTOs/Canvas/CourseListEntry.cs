namespace Constellation.Application.DTOs.Canvas;

using Core.Models.Canvas.Models;

public sealed record CourseListEntry(
    string Name,
    CanvasCourseCode CourseCode);
