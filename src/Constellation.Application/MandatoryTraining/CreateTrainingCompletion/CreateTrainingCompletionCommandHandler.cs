namespace Constellation.Application.MandatoryTraining.CreateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingCompletionCommandHandler 
    : ICommandHandler<CreateTrainingCompletionCommand>
{
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingCompletionCommandHandler(
        ITrainingCompletionRepository trainingCompletionRepository,
        ITrainingModuleRepository trainingModuleRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _trainingCompletionRepository = trainingCompletionRepository;
        _trainingModuleRepository = trainingModuleRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result> Handle(CreateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        // Check that another record does not already exist for this user, module, and date.
        var existingRecord = await _trainingCompletionRepository
            .AnyExistingRecordForTeacherAndDate(
                request.StaffId,
                request.TrainingModuleId,
                request.CompletedDate,
                request.NotRequired,
                cancellationToken);

        if (existingRecord)
        {
            return Result.Failure(DomainErrors.MandatoryTraining.Completion.AlreadyExists);
        }

        var module = await _trainingModuleRepository.GetById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId));
        }

        var recordEntity = TrainingCompletion.Create(
            new TrainingCompletionId(),
            request.StaffId,
            request.TrainingModuleId);

        if (request.NotRequired)
            recordEntity.MarkNotRequired(module);
        else
            recordEntity.SetCompletedDate(request.CompletedDate);

        _trainingCompletionRepository.Insert(recordEntity);

        if (request.File is null)
        {
            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        var fileEntity = new StoredFile
        {
            Name = request.File.FileName,
            FileType = request.File.FileType,
            FileData = request.File.FileData,
            CreatedAt = _dateTimeProvider.Now,
            LinkType = StoredFile.TrainingCertificate,
            LinkId = recordEntity.Id.ToString()
        };

        recordEntity.LinkStoredFile(fileEntity);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}