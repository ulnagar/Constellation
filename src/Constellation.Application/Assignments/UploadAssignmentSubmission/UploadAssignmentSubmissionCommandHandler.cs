namespace Constellation.Application.Assignments.UploadAssignmentSubmission;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UploadAssignmentSubmissionCommandHandler
    : ICommandHandler<UploadAssignmentSubmissionCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public UploadAssignmentSubmissionCommandHandler(
        IAssignmentRepository assignmentRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
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

        Attachment fileEntity = Attachment.CreateAssignmentSubmissionAttachment(
            request.File.FileName,
            request.File.FileType,
            submissionResult.Value.Id.ToString(),
            _dateTime.Now);

        Result attempt = await _attachmentService.StoreAttachmentData(fileEntity, request.File.FileData, false, cancellationToken);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(UploadAssignmentSubmissionCommand), request, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to upload Assignment Submission");

            return Result.Failure(attempt.Error);
        }

        _attachmentRepository.Insert(fileEntity);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
