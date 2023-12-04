﻿namespace Constellation.Application.MandatoryTraining.CreateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingCompletionCommandHandler 
    : ICommandHandler<CreateTrainingCompletionCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateTrainingCompletionCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateTrainingCompletionCommand>();
    }
    public async Task<Result> Handle(CreateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(CreateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Module.NotFound(request.TrainingModuleId), true)
                .Warning("Failed to create Training Completion");

            return Result.Failure(TrainingErrors.Module.NotFound(request.TrainingModuleId));
        }

        // Check that another record does not already exist for this user, module, and date.
        List<TrainingCompletion> records = module.Completions
            .Where(record =>
                record.StaffId == request.StaffId &&
                !record.IsDeleted)
            .ToList();

        TrainingCompletion record = records.MaxBy(record => record.CompletedDate);

        if (record.CompletedDate == request.CompletedDate)
        {
            _logger
                .ForContext(nameof(CreateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Module.NotFound(request.TrainingModuleId), true)
                .Warning("Failed to create Training Completion");

            return Result.Failure(TrainingErrors.Completion.AlreadyExists);
        }

        TrainingCompletion recordEntity = TrainingCompletion.Create(
            request.StaffId,
            request.TrainingModuleId,
            request.CompletedDate);
        
        if (request.File is null)
        {
            module.AddCompletion(recordEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        Attachment fileEntity = Attachment.CreateTrainingCertificateAttachment(
            request.File.FileName,
            request.File.FileType,
            recordEntity.Id.ToString(),
            _dateTime.Now);

        Result attempt = await _attachmentService.StoreAttachmentData(fileEntity, request.File.FileData, false, cancellationToken);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to create Training Completion");

            return Result.Failure(attempt.Error);
        }

        _attachmentRepository.Insert(fileEntity);

        module.AddCompletion(recordEntity);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}