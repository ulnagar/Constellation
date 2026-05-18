namespace Constellation.Application.Domains.Assessments.Assessments.Commands.LinkAssessmentToCanvas;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;

internal sealed class LinkAssessmentToCanvasCommandHandler
: ICommandHandler<LinkAssessmentToCanvasCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ICanvasGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public LinkAssessmentToCanvasCommandHandler(
        IAssessmentRepository assessmentRepository,
        ICanvasGateway gateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(LinkAssessmentToCanvasCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(LinkAssessmentToCanvasCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to add Canvas Link to Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        Result<CanvasAssignmentDto> assignment = await _gateway.GetCourseAssignment(
            request.CanvasCourse, 
            request.CanvasAssignmentId,
            cancellationToken);

        if (assignment.IsFailure)
        {
            _logger
                .ForContext(nameof(LinkAssessmentToCanvasCommand), request, true)
                .ForContext(nameof(Error), assignment.Error, true)
                .Warning("Failed to add Canvas Link to Assessment");

            return Result.Failure(assignment.Error);
        }

        DateTimeOffset forwardDate = assignment.Value.LockDate?.AddDays(-1) ?? assignment.Value.DueDate.AddDays(1);

        assessment.AddCanvasDetails(
            request.CanvasCourse,
            request.CanvasAssignmentId,
            assignment.Value.AllowedAttempts,
            forwardDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
