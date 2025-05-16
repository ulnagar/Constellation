namespace Constellation.Application.Domains.Assignments.Queries.GetRubricAssignmentsFromCourse;

using Abstractions.Messaging;
using Core.Models.Assignments;
using Core.Models.Assignments.Repositories;
using Core.Models.Canvas.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Gateways;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRubricAssignmentsFromCourseQueryHandler
: IQueryHandler<GetRubricAssignmentsFromCourseQuery, List<AssignmentFromCourseResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger _logger;

    public GetRubricAssignmentsFromCourseQueryHandler(
        IOfferingRepository offeringRepository,
        ICanvasGateway canvasGateway,
        IAssignmentRepository assignmentRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _canvasGateway = canvasGateway;
        _assignmentRepository = assignmentRepository;
        _logger = logger
            .ForContext<GetRubricAssignmentsFromCourseQuery>();
    }

    public async Task<Result<List<AssignmentFromCourseResponse>>> Handle(GetRubricAssignmentsFromCourseQuery request, CancellationToken cancellationToken)
    {
        List<AssignmentFromCourseResponse> response = new();

        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(GetRubricAssignmentsFromCourseQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Failed to retrieve Assignments linked to Offering");

            return Result.Failure<List<AssignmentFromCourseResponse>>(OfferingErrors.NotFound(request.OfferingId));
        }
        List<CanvasCourseCode> canvasCourseIds = offering
            .Resources
            .OfType<CanvasCourseResource>()
            .Select(resource => resource.CourseId)
            .Distinct()
            .ToList();

        foreach (CanvasCourseCode courseId in canvasCourseIds)
        {
            List<CanvasAssignmentDto> assignments = await _canvasGateway.GetAllRubricCourseAssignments(courseId, cancellationToken);

            foreach (CanvasAssignmentDto assignment in assignments)
            {
                CanvasAssignment? dbAssignment = await _assignmentRepository.GetByCanvasId(assignment.CanvasId, cancellationToken);

                response.Add(new(
                    assignment.Name,
                    courseId,
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
