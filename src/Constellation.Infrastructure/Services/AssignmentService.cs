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
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using System.Threading.Tasks;

internal class AssignmentService : IAssignmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AssignmentService(
        IStudentRepository studentRepository,
        IAttachmentService attachmentService,
        ICanvasGateway canvasGateway,
        IEmailService emailService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
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

        Student student = await _studentRepository.GetById(submission.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.NotFound(submission.StudentId), true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return Result.Failure<AttachmentResponse>(StudentErrors.NotFound(submission.StudentId));
        }

        // Upload file to Canvas
        // Include error checking/retry on failure
        bool result = await _canvasGateway.UploadAssignmentSubmission(canvasCourseId, assignment.CanvasId, student.StudentReferenceNumber.Number, fileRequest.Value, cancellationToken);

        if (!result)
        {
            _logger
                .ForContext(nameof(Error), SubmissionErrors.UploadFailed)
                .Error("Failed to upload Assignment Submission to Canvas");

            await _emailService.SendAssignmentUploadFailedNotification(
                assignment.Name, 
                assignment.Id, 
                student.Name.DisplayName,
                submission.Id, 
                cancellationToken);

            return Result.Failure(SubmissionErrors.UploadFailed);
        }

        return Result.Success();
    }
}