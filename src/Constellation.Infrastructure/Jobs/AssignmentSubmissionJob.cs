namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Assignments.Services;
using Core.Models.Canvas.Models;
using Core.Models.Offerings.ValueObjects;
using System;
using System.Threading.Tasks;

internal sealed class AssignmentSubmissionJob : IAssignmentSubmissionJob
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IOfferingRepository _courseOfferingRepository;
    private readonly IAssignmentService _assignmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AssignmentSubmissionJob(
        IAssignmentRepository assignmentRepository,
        IOfferingRepository courseOfferingRepository,
        IAssignmentService assignmentService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _assignmentService = assignmentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }


    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        // Scan for any assignments that are due for delayed forwarding today
        // Gather all submissions
        // Forward to Canvas 
        List<CanvasAssignment> assignments = await _assignmentRepository.GetAllDueForUpload(cancellationToken);

        foreach (CanvasAssignment assignment in assignments)
        {
            List<Offering> offerings = await _courseOfferingRepository.GetByCourseId(assignment.CourseId, cancellationToken);

            if (offerings is null)
            {
                _logger
                    .ForContext(nameof(Error), CourseErrors.NotFound(assignment.CourseId), true)
                    .Error("Failed to upload Assignment Submission to Canvas");

                return;
            }

            List<CanvasCourseCode> resources = offerings
                .Where(offering => offering.IsCurrent)
                .SelectMany(offering => offering.Resources)
                .Where(resource => resource.Type == ResourceType.CanvasCourse)
                .Select(resource => ((CanvasCourseResource)resource).CourseId)
                .Distinct()
                .ToList();

            if (!resources.Any())
            {
                _logger
                    .ForContext(nameof(Error), ResourceErrors.NoneOfTypeFound(ResourceType.CanvasCourse), true)
                    .Warning("Failed to upload Assignment Submission to Canvas");

                return;
            }

            List<CanvasAssignmentSubmission> validSubmissions = assignment
                .Submissions
                .Where(submission => !submission.Uploaded)
                .ToList();

            foreach (var submission in validSubmissions)
            {
                Result result = await _assignmentService.UploadSubmissionToCanvas(assignment, submission, resources, cancellationToken);

                if (result.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), result.Error, true)
                        .Warning("Failed to upload Assignment Submission to Canvas");

                    continue;
                }

                assignment.MarkSubmissionUploaded(submission.Id);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

