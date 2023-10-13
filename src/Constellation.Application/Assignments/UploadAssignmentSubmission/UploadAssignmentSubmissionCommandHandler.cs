namespace Constellation.Application.Assignments.UploadAssignmentSubmission;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Shared;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Attachments;
using Core.Models.Attachments.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UploadAssignmentSubmissionCommandHandler
    : ICommandHandler<UploadAssignmentSubmissionCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadAssignmentSubmissionCommandHandler(
        IAssignmentRepository assignmentRepository,
        IAttachmentRepository attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UploadAssignmentSubmissionCommand request, CancellationToken cancellationToken)
    {
        CanvasAssignment assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure(AssignmentErrors.NotFound(request.AssignmentId));

        Result<CanvasAssignmentSubmission> submissionResult = assignment.AddSubmission(
            request.StudentId,
            request.SubmittedBy);

        if (submissionResult.IsFailure)
            return submissionResult;

        Attachment file = new()
        {
            LinkId = submissionResult.Value.Id.ToString(),
            LinkType = AttachmentType.CanvasAssignmentSubmission,
            Name = request.File.FileName,
            FileType = request.File.FileType,
            FileData = request.File.FileData,
            CreatedAt = DateTime.Now
        };

        _attachmentRepository.Insert(file);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
