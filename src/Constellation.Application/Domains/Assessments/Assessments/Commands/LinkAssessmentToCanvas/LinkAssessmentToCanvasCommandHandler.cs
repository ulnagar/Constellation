namespace Constellation.Application.Domains.Assessments.Assessments.Commands.LinkAssessmentToCanvas;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Models.Tutorials.Enums;
using Core.Shared;
using DTOs;
using Interfaces.Configuration;
using Interfaces.Gateways;
using Interfaces.Repositories;
using LinkedSystems.Canvas.Models;
using Microsoft.Extensions.Options;
using Serilog;

internal sealed class LinkAssessmentToCanvasCommandHandler
: ICommandHandler<LinkAssessmentToCanvasCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ICanvasGateway _gateway;
    private readonly CanvasGatewayConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public LinkAssessmentToCanvasCommandHandler(
        IAssessmentRepository assessmentRepository,
        ICanvasGateway gateway,
        IOptions<CanvasGatewayConfiguration> configuration,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _gateway = gateway;
        _configuration = configuration.Value;
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

        Result<CourseListEntry> course = await _gateway.GetCourse(request.CanvasCourse, cancellationToken);

        if (course.IsFailure)
        {
            _logger
                .ForContext(nameof(LinkAssessmentToCanvasCommand), request, true)
                .ForContext(nameof(Error), course.Error, true)
                .Warning("Failed to add Canvas Link to Assessment");

            return Result.Failure(course.Error);
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

        if (assignment.Value.LockDate.HasValue && request.ForwardDate >= DateOnly.FromDateTime(assignment.Value.LockDate.Value.LocalDateTime))
        {
            _logger
                .ForContext(nameof(LinkAssessmentToCanvasCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.ForwardDateAndLockDateConflict(request.ForwardDate, DateOnly.FromDateTime(assignment.Value.LockDate.Value.LocalDateTime)), true)
                .Warning("Failed to add Canvas Link to Assessment");

            return Result.Failure(AssessmentErrors.ForwardDateAndLockDateConflict(request.ForwardDate, DateOnly.FromDateTime(assignment.Value.LockDate.Value.LocalDateTime)));
        }

        DateTimeOffset forwardDate = new(request.ForwardDate.ToDateTime(TimeOnly.MinValue), TimeZoneInfo.Local.GetUtcOffset(request.ForwardDate.ToDateTime(TimeOnly.MinValue)));

        UriBuilder uriBuilder = new(_configuration.ApiEndpoint)
        {
            Path = $"/courses/sis_course_id:{request.CanvasCourse}/assignments/{assignment.Value.CanvasId}",
            Query = string.Empty
        };
        
        Uri canvasAssignmentLink = uriBuilder.Uri;

        assessment.AddCanvasDetails(
            request.CanvasCourse,
            course.Value.Name,
            request.CanvasAssignmentId,
            assignment.Value.AssignmentName,
            assignment.Value.AllowedAttempts,
            canvasAssignmentLink,
            forwardDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
