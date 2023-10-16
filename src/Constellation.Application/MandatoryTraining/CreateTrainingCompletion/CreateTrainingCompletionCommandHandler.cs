namespace Constellation.Application.MandatoryTraining.CreateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingCompletionCommandHandler 
    : ICommandHandler<CreateTrainingCompletionCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateTrainingCompletionCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateTrainingCompletionCommand>();
    }
    public async Task<Result> Handle(CreateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(CreateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId), true)
                .Warning("Failed to create Training Completion");

            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId));
        }

        // Check that another record does not already exist for this user, module, and date.
        List<TrainingCompletion> records = module.Completions
            .Where(record =>
                record.StaffId == request.StaffId &&
                !record.IsDeleted)
            .ToList();

        TrainingCompletion record = records.MaxBy(record =>
            record.CompletedDate ?? record.CreatedAt);

        if (record is not null &&
            ((request.NotRequired && record.NotRequired) ||
             (!request.NotRequired && record.CompletedDate!.Value == request.CompletedDate)))
        {
            _logger
                .ForContext(nameof(CreateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId), true)
                .Warning("Failed to create Training Completion");

            return Result.Failure(DomainErrors.MandatoryTraining.Completion.AlreadyExists);
        }

        TrainingCompletion recordEntity = TrainingCompletion.Create(
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