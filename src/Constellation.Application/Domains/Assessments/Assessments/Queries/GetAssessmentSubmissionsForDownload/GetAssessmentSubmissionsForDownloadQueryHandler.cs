namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentSubmissionsForDownload;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Enums;
using Core.Models.Attachments.Services;
using Core.Shared;
using DTOs;
using Serilog;
using System.IO.Compression;
using System.Net.Mime;

internal sealed class GetAssessmentSubmissionsForDownloadQueryHandler
: IQueryHandler<GetAssessmentSubmissionsForDownloadQuery, FileDto>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger _logger;

    public GetAssessmentSubmissionsForDownloadQueryHandler(
        IAssessmentRepository assessmentRepository,
        IAttachmentService attachmentService,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _attachmentService = attachmentService;
        _logger = logger
            .ForContext<GetAssessmentSubmissionsForDownloadQuery>();
    }

    public async Task<Result<FileDto>> Handle(GetAssessmentSubmissionsForDownloadQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentSubmissionsForDownloadQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to download all Assessment Submissions");

            return Result.Failure<FileDto>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<AttachmentResponse> files = [];

        foreach (AssessmentStudent student in assessment.Students.Where(student => !student.IsDeleted))
        {
            IOrderedEnumerable<AssessmentSubmission> submissions = student.Submissions.OrderBy(entry => entry.SubmittedAt);

            int submissionNumber = 1;

            foreach (AssessmentSubmission submission in submissions)
            {
                Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(
                    AttachmentType.AssessmentSubmission,
                    submission.Id.ToString(),
                    cancellationToken);

                if (fileRequest.IsFailure)
                {
                    continue;
                }
                
                string extension = fileRequest.Value.FileType == MediaTypeNames.Application.Pdf
                    ? "pdf"
                    : fileRequest.Value.FileName.Split('.').Last();

                AttachmentResponse updatedResponse = fileRequest.Value with
                {
                    FileName = $"{student.Student.SortOrder} - Attempt {submissionNumber}.{extension}"
                };

                files.Add(updatedResponse);

                submissionNumber++;
            }
        }

        if (files.Count == 0)
        {
            _logger
                .ForContext(nameof(GetAssessmentSubmissionsForDownloadQuery), request, true)
                .ForContext(nameof(Error), AssessmentSubmissionErrors.NoneFound, true)
                .Warning("Failed to download all Assessment Submissions");

            return Result.Failure<FileDto>(AssessmentSubmissionErrors.NoneFound);
        }

        using MemoryStream memoryStream = new();
        await using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (AttachmentResponse file in files)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                await using Stream entryStream = await zipArchiveEntry.OpenAsync(cancellationToken);
                ReadOnlyMemory<byte> memory = file.FileData?.AsMemory() ?? default;
                await entryStream.WriteAsync(memory, cancellationToken);
            }
        }

        memoryStream.Position = 0;

        FileDto response = new()
        {
            FileData = memoryStream.ToArray(),
            FileName = $"{assessment.Name}.zip",
            FileType = MediaTypeNames.Application.Zip
        };

        return response;
    }
}
