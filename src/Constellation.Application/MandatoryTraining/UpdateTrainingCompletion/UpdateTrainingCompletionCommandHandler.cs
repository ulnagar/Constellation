namespace Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingCompletionCommandHandler 
    : ICommandHandler<UpdateTrainingCompletionCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public UpdateTrainingCompletionCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IUnitOfWork unitOfWork,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _dateTime = dateTime;
        _logger = logger.ForContext<UpdateTrainingCompletionCommand>();
    }

    public async Task<Result> Handle(UpdateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(UpdateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Module.NotFound(request.TrainingModuleId), true)
                .Warning("Failed to update Training Completion record");

            return Result.Failure(TrainingErrors.Module.NotFound(request.TrainingModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger
                .ForContext(nameof(UpdateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Completion.NotFound(request.CompletionId), true)
                .Warning("Failed to update Training Completion record");

            return Result.Failure(TrainingErrors.Completion.NotFound(request.CompletionId));
        }

        if (request.File is not null)
        {
            Attachment existingFile = await _attachmentRepository.GetTrainingCertificateByLinkId(record.Id.ToString()!, cancellationToken);

            if (existingFile is not null)
                _attachmentService.DeleteAttachment(existingFile);

            Attachment fileEntity = Attachment.CreateTrainingCertificateAttachment(
                request.File.FileName,
                request.File.FileType,
                record.Id.ToString(),
                _dateTime.Now);

            Result attempt = await _attachmentService.StoreAttachmentData(fileEntity, request.File.FileData, true, cancellationToken);

            if (attempt.IsFailure)
            {
                return attempt;
            }

            _attachmentRepository.Insert(fileEntity);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
