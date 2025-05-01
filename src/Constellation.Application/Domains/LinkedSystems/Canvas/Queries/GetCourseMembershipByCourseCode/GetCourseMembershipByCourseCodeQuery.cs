namespace Constellation.Application.Domains.LinkedSystems.Canvas.Queries.GetCourseMembershipByCourseCode;

using Abstractions.Messaging;
using Core.Models.Canvas.Models;
using System.Collections.Generic;

public sealed record GetCourseMembershipByCourseCodeQuery(
    CanvasCourseCode CourseCode)
    : IQuery<List<CanvasCourseMembership>>;
