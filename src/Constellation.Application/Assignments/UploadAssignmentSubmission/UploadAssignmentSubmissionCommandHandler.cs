﻿namespace Constellation.Application.Assignments.UploadAssignmentSubmission;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UploadAssignmentSubmissionCommandHandler
    : ICommandHandler<UploadAssignmentSubmissionCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadAssignmentSubmissionCommandHandler(
        IAssignmentRepository assignmentRepository,
        IStoredFileRepository storedFileRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _storedFileRepository = storedFileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UploadAssignmentSubmissionCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure(DomainErrors.Assignments.Assignment.NotFound(request.AssignmentId));

        var submissionResult = assignment.AddSubmission(
            new AssignmentSubmissionId(),
            request.StudentId);

        if (submissionResult.IsFailure)
            return submissionResult;

        var file = new StoredFile
        {
            LinkId = submissionResult.Value.Id.ToString(),
            LinkType = StoredFile.CanvasAssignmentSubmission,
            Name = request.File.FileName,
            FileType = request.File.FileType,
            FileData = request.File.FileData,
            CreatedAt = DateTime.Now
        };

        _storedFileRepository.Insert(file);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}