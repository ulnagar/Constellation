namespace Constellation.Application.Assignments.GetAssignmentSubmissionFile;

using Attachments.GetAttachmentFile;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Shared;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAssignmentSubmissionFileQueryHandler
    : IQueryHandler<GetAssignmentSubmissionFileQuery, FileDto>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAttachmentService _attachmentService;

    public GetAssignmentSubmissionFileQueryHandler(
        IAssignmentRepository assignmentRepository,
        IAttachmentService attachmentService)
    {
        _assignmentRepository = assignmentRepository;
        _attachmentService = attachmentService;
    }

    public async Task<Result<FileDto>> Handle(GetAssignmentSubmissionFileQuery request, CancellationToken cancellationToken)
    {
        CanvasAssignment assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure<FileDto>(AssignmentErrors.NotFound(request.AssignmentId));

        CanvasAssignmentSubmission submission = assignment.Submissions.FirstOrDefault(entry => entry.Id == request.SubmissionId);

        if (submission is null)
            return Result.Failure<FileDto>(SubmissionErrors.NotFound(request.SubmissionId));

        Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(AttachmentType.CanvasAssignmentSubmission, submission.Id.ToString(), cancellationToken);

        if (fileRequest.IsFailure)
            return Result.Failure<FileDto>(fileRequest.Error);

        FileDto entry = new()
        {
            FileData = fileRequest.Value.FileData,
            FileType = fileRequest.Value.FileType,
            FileName = fileRequest.Value.FileName
        };

        return entry;
    }
}
