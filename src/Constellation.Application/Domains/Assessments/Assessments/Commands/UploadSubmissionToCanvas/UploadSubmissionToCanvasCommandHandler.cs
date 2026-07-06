namespace Constellation.Application.Domains.Assessments.Assessments.Commands.UploadSubmissionToCanvas;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Constellation.Core.Models.Assessments.Repositories;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attachments.Enums;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Attachments.Services;
using Core.Models.Students;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;

internal sealed class UploadSubmissionToCanvasCommandHandler
: ICommandHandler<UploadSubmissionToCanvasCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UploadSubmissionToCanvasCommandHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        IAttachmentService attachmentService,
        ICanvasGateway canvasGateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _attachmentService = attachmentService;
        _canvasGateway = canvasGateway;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UploadSubmissionToCanvasCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment?.CanvasCourse == null || !assessment.CanvasAssignmentId.HasValue)
        {
            _logger
                .ForContext(nameof(Assessment), assessment?.Name)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to upload Assessment Submission to Canvas");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentStudent? assessmentStudent = assessment.Students
            .FirstOrDefault(student => student.Submissions.Any(submission => submission.Id == request.SubmissionId));

        if (assessmentStudent is null)
        {
            _logger
                .ForContext(nameof(Assessment), assessment.Name)
                .ForContext(nameof(Error), AssessmentSubmissionErrors.NotFound(request.SubmissionId), true)
                .Warning("Failed to upload Assessment Submission to Canvas");

            return Result.Failure(AssessmentSubmissionErrors.NotFound(request.SubmissionId));
        }

        Student? student = await _studentRepository.GetById(assessmentStudent.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(Assessment), assessment.Name)
                .ForContext(nameof(Error), StudentErrors.NotFound(assessmentStudent.StudentId), true)
                .Warning("Failed to upload Assessment Submission to Canvas");

            return Result.Failure(StudentErrors.NotFound(assessmentStudent.StudentId));
        }

        AssessmentSubmission? submission = assessmentStudent.Submissions
            .FirstOrDefault(entry => entry.Id == request.SubmissionId);

        if (submission is null)
        {
            _logger
                .ForContext(nameof(Assessment), assessment.Name)
                .ForContext(nameof(Student), student.Name.DisplayName)
                .ForContext(nameof(Error), AssessmentSubmissionErrors.NoneFound, true)
                .Warning("Failed to upload Assessment Submission to Canvas");

            return Result.Failure(AssessmentSubmissionErrors.NoneFound);
        }

        Result<AttachmentResponse> file = await _attachmentService.GetAttachmentFile(AttachmentType.AssessmentSubmission, submission.Id.ToString(), cancellationToken);

        if (file.IsFailure)
        {
            _logger
                .ForContext(nameof(Assessment), assessment.Name)
                .ForContext(nameof(Student), student.Name.DisplayName)
                .ForContext(nameof(Error), file.Error, true)
                .Warning("Failed to upload Assessment Submission to Canvas");

            return Result.Failure(file.Error);
        }

        Result result = await _canvasGateway.UploadAssignmentSubmission(
            assessment.CanvasCourse.Value,
            assessment.CanvasAssignmentId.Value,
            student.StudentReferenceNumber,
            file.Value,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Assessment), assessment.Name)
                .ForContext(nameof(Student), student.Name.DisplayName)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to upload Assessment Submission to Canvas");

            return result;
        }

        submission.MarkForwarded();
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
