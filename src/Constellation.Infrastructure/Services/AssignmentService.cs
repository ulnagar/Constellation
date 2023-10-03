namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Assignments.Services;
using Core.Shared;
using System.Threading.Tasks;

internal class AssignmentService : IAssignmentService
{
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AssignmentService(
        IStoredFileRepository storedFileRepository,
        ICanvasGateway canvasGateway,
        IEmailService emailService,
        ILogger logger)
    {
        _storedFileRepository = storedFileRepository;
        _canvasGateway = canvasGateway;
        _emailService = emailService;
        _logger = logger.ForContext<IAssignmentService>();
    }

    public async Task<bool> UploadSubmissionToCanvas(
        CanvasAssignment assignment,
        CanvasAssignmentSubmission submission,
        string canvasCourseId,
        CancellationToken cancellationToken = default)
    {
        StoredFile file = await _storedFileRepository.GetAssignmentSubmissionByLinkId(submission.Id.Value.ToString(), cancellationToken);

        if (file is null)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Documents.AssignmentSubmission.NotFound(submission.Id.Value.ToString()), true)
                .Warning("Failed to upload Assignment Submission to Canvas");
            return true;
        }

        // Upload file to Canvas
        // Include error checking/retry on failure
        bool result = await _canvasGateway.UploadAssignmentSubmission(canvasCourseId, assignment.CanvasId, submission.StudentId, file);

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
        }

        return false;
    }
}