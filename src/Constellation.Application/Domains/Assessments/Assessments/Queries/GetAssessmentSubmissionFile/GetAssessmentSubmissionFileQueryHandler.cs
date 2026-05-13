namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentSubmissionFile;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Constellation.Core.Models.Assessments.Repositories;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attachments.Enums;
using Constellation.Core.Models.Attachments.Services;
using Core.Models.Attachments.Errors;
using Core.Shared;
using DTOs;
using Serilog;
using System.Net.Mime;

internal sealed class GetAssessmentSubmissionFileQueryHandler
: IQueryHandler<GetAssessmentSubmissionFileQuery, FileDto>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger _logger;

    public GetAssessmentSubmissionFileQueryHandler(
        IAssessmentRepository assessmentRepository,
        IAttachmentService attachmentService,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _attachmentService = attachmentService;
        _logger = logger
            .ForContext<GetAssessmentSubmissionFileQuery>();
    }

    public async Task<Result<FileDto>> Handle(GetAssessmentSubmissionFileQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentSubmissionFileQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to download Assessment Submission");

            return Result.Failure<FileDto>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentStudent? student = assessment.Students
            .FirstOrDefault(student =>
                student.Submissions.Any(submission => submission.Id == request.SubmissionId));

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentSubmissionFileQuery), request, true)
                .ForContext(nameof(Error), AssessmentSubmissionErrors.NotFound(request.SubmissionId), true)
                .Warning("Failed to download Assessment Submission");

            return Result.Failure<FileDto>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        IOrderedEnumerable<AssessmentSubmission> submissions = student.Submissions.OrderBy(entry => entry.SubmittedAt);

        int submissionNumber = 1;
        AttachmentResponse? updatedResponse = null;

        foreach (AssessmentSubmission submission in submissions)
        {
            if (submission.Id != request.SubmissionId)
            {
                submissionNumber++;
                continue;
            }

            Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(
                AttachmentType.AssessmentSubmission,
                submission.Id.ToString(),
                cancellationToken);

            if (fileRequest.IsFailure)
            {
                submissionNumber++;
                continue;
            }

            string extension = fileRequest.Value.FileType == MediaTypeNames.Application.Pdf
                ? "pdf"
                : fileRequest.Value.FileName.Split('.').Last();

            updatedResponse = fileRequest.Value with
            {
                FileName = $"{student.Student.SortOrder} - {assessment.Name} - Attempt {submissionNumber}.{extension}"
            };
        }

        if (updatedResponse is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentSubmissionFileQuery), request, true)
                .ForContext(nameof(Error), AttachmentErrors.NotFound(AttachmentType.AssessmentSubmission, request.SubmissionId.ToString()), true)
                .Warning("Failed to download Assessment Submission");

            return Result.Failure<FileDto>(AttachmentErrors.NotFound(AttachmentType.AssessmentSubmission, request.SubmissionId.ToString()));
        }
        
        FileDto response = new()
        {
            FileData = updatedResponse.FileData,
            FileName = updatedResponse.FileName,
            FileType = updatedResponse.FileType
        };

        return response;
    }
}
