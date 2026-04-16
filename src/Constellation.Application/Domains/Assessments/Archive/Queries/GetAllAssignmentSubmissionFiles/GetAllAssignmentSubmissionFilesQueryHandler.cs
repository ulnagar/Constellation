namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAllAssignmentSubmissionFiles;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attachments.Enums;
using Constellation.Core.Models.Attachments.Services;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Assessments.Archive;
using Core.Models.Assessments.Archive.Errors;
using Core.Models.Assessments.Archive.Repositories;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllAssignmentSubmissionFilesQueryHandler
    : IQueryHandler<GetAllAssignmentSubmissionFilesQuery, FileDto>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetAllAssignmentSubmissionFilesQueryHandler(
        IAssignmentRepository assignmentRepository,
        IAttachmentService attachmentService,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _attachmentService = attachmentService;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetAllAssignmentSubmissionFilesQuery>();
    }

    public async Task<Result<FileDto>> Handle(GetAllAssignmentSubmissionFilesQuery request, CancellationToken cancellationToken)
    {
        CanvasAssignment? assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(GetAllAssignmentSubmissionFilesQuery), request, true)
                .ForContext(nameof(Error), AssignmentErrors.NotFound(request.AssignmentId), true)
                .Warning("Failed to retrieve Assignment Submission files for Student");

            return Result.Failure<FileDto>(AssignmentErrors.NotFound(request.AssignmentId));
        }

        List<IGrouping<StudentId, CanvasAssignmentSubmission>> submissions = assignment.Submissions.GroupBy(submission => submission.StudentId).ToList();

        List<AttachmentResponse> files = new();

        foreach (IGrouping<StudentId, CanvasAssignmentSubmission> studentSubmissions in submissions)
        {
            Student? student = await _studentRepository.GetById(studentSubmissions.Key, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(GetAllAssignmentSubmissionFilesQuery), request, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(studentSubmissions.Key), true)
                    .Warning("Failed to retrieve Assignment Submission files for Student");

                continue;
            }

            List<Offering> offerings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

            Offering? offering = offerings.FirstOrDefault(offering => offering.CourseId == assignment.CourseId);

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(GetAllAssignmentSubmissionFilesQuery), request, true)
                    .ForContext(nameof(Error), OfferingErrors.NotFoundForStudent, true)
                    .Warning("Failed to retrieve Assignment Submission files for Student");

                continue;
            }

            CanvasAssignmentSubmission submission = studentSubmissions.FirstOrDefault(submission =>
                submission.Attempt == studentSubmissions.Max(submission => submission.Attempt));

            Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(
                AttachmentType.CanvasAssignmentSubmission, 
                submission!.Id.ToString(), 
                cancellationToken);

            string extension = fileRequest.Value.FileType == MediaTypeNames.Application.Pdf
                ? "pdf"
                : fileRequest.Value.FileName.Split('.').Last();

            AttachmentResponse updatedResponse = fileRequest.Value with
            {
                FileName = $"{student.Name.SortOrder} - {offering.Name} - {submission.Attempt}.{extension}"
            };

            files.Add(updatedResponse);
        }

        // Create Zip File
        using MemoryStream memoryStream = new();
        await using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create))
        {
            foreach (AttachmentResponse file in files)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                await using StreamWriter streamWriter = new(await zipArchiveEntry.OpenAsync(cancellationToken));
                await streamWriter.BaseStream.WriteAsync(file.FileData, cancellationToken);
            }
        }

        FileDto response = new()
        {
            FileData = memoryStream.ToArray(), 
            FileName = $"{assignment.Name}.zip", 
            FileType = MediaTypeNames.Application.Zip
        };

        return response;
    }
}
