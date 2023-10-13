namespace Constellation.Application.Assignments.ResendAssignmentSubmissionToCanvas;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Assignments.Services;
using Core.Models.Offerings;
using Core.Models.Offerings.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ResendAssignmentSubmissionToCanvasCommandHandler
    : ICommandHandler<ResendAssignmentSubmissionToCanvasCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IAssignmentService _assignmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ResendAssignmentSubmissionToCanvasCommandHandler(
        IAssignmentRepository assignmentRepository,
        IOfferingRepository offeringRepository,
        IAssignmentService assignmentService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _offeringRepository = offeringRepository;
        _assignmentService = assignmentService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResendAssignmentSubmissionToCanvasCommand>();
    }

    public async Task<Result> Handle(ResendAssignmentSubmissionToCanvasCommand request, CancellationToken cancellationToken)
    {
        CanvasAssignment assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(ResendAssignmentSubmissionToCanvasCommand), request, true)
                .ForContext(nameof(Error), AssignmentErrors.NotFound(request.AssignmentId), true)
                .Warning("Failed to upload Assignment Submission to Canvas");
            
            return Result.Failure(AssignmentErrors.NotFound(request.AssignmentId));
        }

        List<Offering> offerings = await _offeringRepository.GetByCourseId(assignment.CourseId, cancellationToken);

        if (offerings is null)
        {
            _logger
                .ForContext(nameof(ResendAssignmentSubmissionToCanvasCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(assignment.CourseId), true)
                .Error("Failed to upload Assignment Submission to Canvas");

            return Result.Failure(CourseErrors.NotFound(assignment.CourseId));
        }

        List<string> resources = offerings
            .SelectMany(offering => offering.Resources)
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => ((CanvasCourseResource)resource).CourseId)
            .Distinct()
            .ToList();

        if (!resources.Any())
        {
            _logger
                .ForContext(nameof(ResendAssignmentSubmissionToCanvasCommand), request, true)
                .ForContext(nameof(Error), ResourceErrors.NoneOfTypeFound(ResourceType.CanvasCourse), true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return Result.Failure(ResourceErrors.NoneOfTypeFound(ResourceType.CanvasCourse));
        }

        string canvasCourseId = resources.First();

        CanvasAssignmentSubmission submission = assignment.Submissions.FirstOrDefault(submission => submission.Id == request.SubmissionId);

        if (submission is null)
        {
            _logger
                .ForContext(nameof(ResendAssignmentSubmissionToCanvasCommand), request, true)
                .ForContext(nameof(Error), SubmissionErrors.NotFound(request.SubmissionId), true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return Result.Failure(SubmissionErrors.NotFound(request.SubmissionId));
        }

        Result result = await _assignmentService.UploadSubmissionToCanvas(assignment, submission, canvasCourseId, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ResendAssignmentSubmissionToCanvasCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return Result.Failure(result.Error);
        }

        assignment.MarkSubmissionUploaded(submission.Id);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
