namespace Constellation.Application.Domains.LinkedSystems.Canvas.Commands.ExportCanvasRubricResults;

using Abstractions.Messaging;
using Core.Models.Canvas.Models;
using Core.Models.Offerings.Identifiers;
using DTOs;

public sealed record ExportCanvasRubricResultsQuery(
    OfferingId OfferingId,
    CanvasCourseCode CourseCode,
    int CanvasAssignmentId,
    string CanvasAssignmentName)
    : IQuery<FileDto>;
