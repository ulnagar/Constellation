namespace Constellation.Application.Domains.LinkedSystems.Canvas.Queries.GetCourseMembershipByCourseCode;

using Core.Models.Canvas.Models;

public sealed record CanvasCourseMembership(
    CanvasCourseCode CanvasCourseCode,
    string UserId,
    CanvasSectionCode SectionId,
    CanvasPermissionLevel PermissionLevel);
