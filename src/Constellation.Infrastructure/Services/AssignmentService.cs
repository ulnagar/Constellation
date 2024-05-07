namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Assignments.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Canvas.Models;
using Core.Shared;
using System.Threading.Tasks;

internal class AssignmentService : IAssignmentService
{
    private readonly IAttachmentService _attachmentService;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AssignmentService(
        IAttachmentService attachmentService,
        ICanvasGateway canvasGateway,
        IEmailService emailService,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _canvasGateway = canvasGateway;
        _emailService = emailService;
        _logger = logger.ForContext<IAssignmentService>();
    }

    public async Task<Result> UploadSubmissionToCanvas(
        CanvasAssignment assignment,
        CanvasAssignmentSubmission submission,
        CanvasCourseCode canvasCourseId,
        CancellationToken cancellationToken = default)
    {
        Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(
            AttachmentType.CanvasAssignmentSubmission ,
            submission.Id.Value.ToString(), 
            cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return fileRequest;
        }

        // Upload file to Canvas
        // Include error checking/retry on failure
        bool result = await _canvasGateway.UploadAssignmentSubmission(canvasCourseId, assignment.CanvasId, submission.StudentId, fileRequest.Value, cancellationToken);

        if (!result)
        {
            _logger
                .ForContext(nameof(Error), SubmissionErrors.UploadFailed)
                .Error("Failed to upload Assignment Submission to Canvas");

            await _emailService.SendAssignmentUploadFailedNotification(
                assignment.Name, 
                assignment.Id, 
                submission.StudentId,
                submission.Id, 
                cancellationToken);

            return Result.Failure(SubmissionErrors.UploadFailed);
        }

        return Result.Success();
    }
}