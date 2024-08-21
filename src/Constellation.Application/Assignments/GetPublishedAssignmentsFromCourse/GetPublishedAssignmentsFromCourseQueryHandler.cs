namespace Constellation.Application.Assignments.GetPublishedAssignmentsFromCourse;

using Abstractions.Messaging;
using Core.Models.Assignments;
using Core.Models.Assignments.Repositories;
using Core.Models.Canvas.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Shared;
using DTOs;
using Interfaces.Gateways;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetPublishedAssignmentsFromCourseQueryHandler
    : IQueryHandler<GetPublishedAssignmentsFromCourseQuery, List<AssignmentFromCourseResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger _logger;

    public GetPublishedAssignmentsFromCourseQueryHandler(
        IOfferingRepository offeringRepository,
        ICanvasGateway canvasGateway,
        IAssignmentRepository assignmentRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _canvasGateway = canvasGateway;
        _assignmentRepository = assignmentRepository;
        _logger = logger.ForContext<GetPublishedAssignmentsFromCourseQuery>();
    }

    public async Task<Result<List<AssignmentFromCourseResponse>>> Handle(GetPublishedAssignmentsFromCourseQuery request, CancellationToken cancellationToken)
    {
        List<AssignmentFromCourseResponse> response = new();

        List<Offering> offerings = await _offeringRepository.GetActiveByCourseId(request.CourseId, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetPublishedAssignmentsFromCourseQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundInCourse(request.CourseId), true)
                .Warning("Failed to retrieve Assignments linked to Course");

            return Result.Failure<List<AssignmentFromCourseResponse>>(OfferingErrors.NotFoundInCourse(request.CourseId));
        }

        List<CanvasCourseCode> canvasCourseIds = offerings
            .SelectMany(offering => offering.Resources)
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .Select(resource => resource.CourseId)
            .Distinct()
            .ToList();

        foreach (CanvasCourseCode courseId in canvasCourseIds)
        {
            List<CanvasAssignmentDto> assignments = await _canvasGateway.GetAllCourseAssignments(courseId, cancellationToken);

            foreach (CanvasAssignmentDto assignment in assignments)
            {
                CanvasAssignment? dbAssignment = await _assignmentRepository.GetByCanvasId(assignment.CanvasId, cancellationToken);

                response.Add(new(
                    assignment.Name,
                    assignment.CanvasId,
                    assignment.DueDate,
                    assignment.LockDate,
                    assignment.UnlockDate,
                    assignment.AllowedAttempts,
                    dbAssignment is not null));
            }
        }

        return response;
    }
}
