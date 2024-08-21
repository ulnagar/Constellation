namespace Constellation.Application.Assignments.GetUploadAssignmentsFromCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Assignments.Models;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Canvas.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadAssignmentsFromCourseQueryHandler
    : IQueryHandler<GetUploadAssignmentsFromCourseQuery, List<AssignmentFromCourseResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger _logger;

    public GetUploadAssignmentsFromCourseQueryHandler(
        IOfferingRepository offeringRepository,
        ICanvasGateway canvasGateway,
        IAssignmentRepository assignmentRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _canvasGateway = canvasGateway;
        _assignmentRepository = assignmentRepository;
        _logger = logger.ForContext<GetUploadAssignmentsFromCourseQuery>();
    }

    public async Task<Result<List<AssignmentFromCourseResponse>>> Handle(GetUploadAssignmentsFromCourseQuery request, CancellationToken cancellationToken)
    {
        List<AssignmentFromCourseResponse> response = new();
        
        List<Offering> offerings = await _offeringRepository.GetActiveByCourseId(request.CourseId, cancellationToken);
        
        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetUploadAssignmentsFromCourseQuery), request, true)
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
            List<CanvasAssignmentDto> assignments = await _canvasGateway.GetAllUploadCourseAssignments(courseId, cancellationToken);

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
