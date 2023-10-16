namespace Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingCompletionCommandHandler 
    : ICommandHandler<UpdateTrainingCompletionCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public UpdateTrainingCompletionCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IUnitOfWork unitOfWork,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _unitOfWork = unitOfWork;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _dateTime = dateTime;
        _logger = logger.ForContext<UpdateTrainingCompletionCommand>();
    }

    public async Task<Result> Handle(UpdateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(UpdateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId), true)
                .Warning("Failed to update Training Completion record");

            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger
                .ForContext(nameof(UpdateTrainingCompletionCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.MandatoryTraining.Completion.NotFound(request.CompletionId), true)
                .Warning("Failed to update Training Completion record");

            return Result.Failure(DomainErrors.MandatoryTraining.Completion.NotFound(request.CompletionId));
        }

        if (!string.IsNullOrWhiteSpace(request.StaffId))
            record.UpdateStaffMember(request.StaffId);

        if (request.TrainingModuleId is not null)
            record.UpdateTrainingModule(request.TrainingModuleId);

        if (request.NotRequired)
            record.MarkNotRequired(module);
        else
            record.SetCompletedDate(request.CompletedDate);

        if (request.File is not null)
        {
            Attachment existingFile = await _attachmentRepository.GetTrainingCertificateByLinkId(record.Id.ToString(), cancellationToken);

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
