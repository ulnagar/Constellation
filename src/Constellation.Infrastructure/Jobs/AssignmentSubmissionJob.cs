namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Gateways;
using Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Enums;
using Core.Models.Attachments.Services;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using System;
using System.Threading.Tasks;

internal sealed class AssignmentSubmissionJob : IAssignmentSubmissionJob
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AssignmentSubmissionJob(
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


    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        // Scan for any assignments that are due for delayed forwarding today
        // Gather all submissions
        // Forward to Canvas 
        List<Assessment> assessments = await _assessmentRepository.GetAllDueForUploadToday(cancellationToken);

        foreach (Assessment assessment in assessments)
        {
            if (!assessment.CanvasCourse.HasValue || !assessment.CanvasAssignmentId.HasValue)
                continue;

            foreach (AssessmentStudent assessmentStudent in assessment.Students)
            {
                Student? student = await _studentRepository.GetById(assessmentStudent.StudentId, cancellationToken);

                if (student is null)
                {
                    _logger
                        .ForContext(nameof(Assessment), assessment.Name)
                        .ForContext(nameof(Error), StudentErrors.NotFound(assessmentStudent.StudentId), true)
                        .Warning("Failed to upload Assessment Submission to Canvas");

                    continue;
                }

                AssessmentSubmission? submission = assessmentStudent.Submissions
                    .Where(entry => entry.ForwardedAt is null)
                    .OrderByDescending(entry => entry.SubmittedAt)
                    .FirstOrDefault();

                if (submission is null)
                {
                    _logger
                        .ForContext(nameof(Assessment), assessment.Name)
                        .ForContext(nameof(Student), student.Name.DisplayName)
                        .ForContext(nameof(Error), AssessmentSubmissionErrors.NoneFound, true)
                        .Warning("Failed to upload Assessment Submission to Canvas");

                    continue;
                }

                Result<AttachmentResponse> file = await _attachmentService.GetAttachmentFile(AttachmentType.AssessmentSubmission, submission.Id.ToString(), cancellationToken);

                if (file.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Assessment), assessment.Name)
                        .ForContext(nameof(Student), student.Name.DisplayName)
                        .ForContext(nameof(Error), file.Error, true)
                        .Warning("Failed to upload Assessment Submission to Canvas");

                    continue;
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

                    continue;
                }
                    
                submission.MarkForwarded();
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}

