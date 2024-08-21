namespace Constellation.Application.Canvas.ExportCanvasRubricResults;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using DTOs;

public sealed record ExportCanvasRubricResultsQuery(
    OfferingId OfferingId,
    int CanvasAssignmentId,
    string CanvasAssignmentName)
    : IQuery<FileDto>;
