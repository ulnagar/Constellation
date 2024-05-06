namespace Constellation.Application.Canvas.GetCourseMembershipByCourseCode;

using Abstractions.Messaging;
using Core.Models.Canvas.Models;
using System.Collections.Generic;

public sealed record GetCourseMembershipByCourseCodeQuery(
    CanvasCourseCode CourseCode)
    : IQuery<List<CanvasCourseMembership>>;
