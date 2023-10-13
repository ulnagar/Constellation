﻿namespace Constellation.Application.MandatoryTraining.CreateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Core.Models.Attachments;
using Core.Models.Attachments.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingCompletionCommandHandler 
    : ICommandHandler<CreateTrainingCompletionCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingCompletionCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result> Handle(CreateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId));
        }

        // Check that another record does not already exist for this user, module, and date.
        List<TrainingCompletion> records = module.Completions
            .Where(record =>
                record.StaffId == request.StaffId &&
                !record.IsDeleted)
            .ToList();

        TrainingCompletion record = records
            .OrderByDescending(record =>
                (record.CompletedDate.HasValue) ? record.CompletedDate.Value : record.CreatedAt)
            .FirstOrDefault();

        if (record is not null)
        {
            if ((request.NotRequired && record.NotRequired) ||
                (!request.NotRequired && record.CompletedDate.Value == request.CompletedDate))
            {
                return Result.Failure(DomainErrors.MandatoryTraining.Completion.AlreadyExists);
            }
        }

        TrainingCompletion recordEntity = TrainingCompletion.Create(
            new TrainingCompletionId(),
            request.StaffId,
            request.TrainingModuleId);

        if (request.NotRequired)
            recordEntity.MarkNotRequired(module);
        else
            recordEntity.SetCompletedDate(request.CompletedDate);

        if (request.File is null)
        {
            module.AddCompletion(recordEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        Attachment fileEntity = new()
        {
            Name = request.File.FileName,
            FileType = request.File.FileType,
            FileData = request.File.FileData,
            CreatedAt = _dateTimeProvider.Now,
            LinkType = AttachmentType.TrainingCertificate,
            LinkId = recordEntity.Id.ToString()
        };

        recordEntity.LinkStoredFile(fileEntity);
        module.AddCompletion(recordEntity);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}