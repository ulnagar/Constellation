namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Attachments.Repository;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Assignments.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using System.Threading.Tasks;

internal class AssignmentService : IAssignmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AssignmentService(
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        ICanvasGateway canvasGateway,
        IEmailService emailService,
        ILogger logger)
    {
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _canvasGateway = canvasGateway;
        _emailService = emailService;
        _logger = logger.ForContext<IAssignmentService>();
    }

    public async Task<Result> UploadSubmissionToCanvas(
        CanvasAssignment assignment,
        CanvasAssignmentSubmission submission,
        string canvasCourseId,
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
        bool result = await _canvasGateway.UploadAssignmentSubmission(canvasCourseId, assignment.CanvasId, submission.StudentId, fileRequest.Value);

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