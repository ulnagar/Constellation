namespace Constellation.Application.Domains.LinkedSystems.Canvas.Commands.ExportCanvasAssignmentComments;

using Abstractions.Messaging;
using Core.Models.Canvas.Models;
using Core.Models.Offerings.Identifiers;
using DTOs;

public sealed record ExportCanvasAssignmentCommentsQuery(
    OfferingId OfferingId,
    CanvasCourseCode CourseCode,
    int CanvasAssignmentId,
    string CanvasAssignmentName)
    : IQuery<FileDto>;