namespace Constellation.Application.Canvas.ExportCanvasAssignmentComments;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Canvas.Models;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record ExportCanvasAssignmentCommentsQuery(
    OfferingId OfferingId,
    CanvasCourseCode CourseCode,
    int CanvasAssignmentId,
    string CanvasAssignmentName)
    : IQuery<FileDto>;