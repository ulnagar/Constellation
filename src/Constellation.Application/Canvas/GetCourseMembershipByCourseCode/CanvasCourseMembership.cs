namespace Constellation.Application.Canvas.GetCourseMembershipByCourseCode;

using Core.Models.Canvas.Models;

public sealed record CanvasCourseMembership(
    CanvasCourseCode CanvasCourseCode,
    string UserId,
    string SectionId,
    CanvasPermissionLevel PermissionLevel);
