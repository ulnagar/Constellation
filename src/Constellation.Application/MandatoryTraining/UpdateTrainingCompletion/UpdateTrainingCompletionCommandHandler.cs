namespace Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingCompletionCommandHandler 
    : ICommandHandler<UpdateTrainingCompletionCommand>
{
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateTrainingCompletionCommandHandler(
        ITrainingCompletionRepository trainingCompletionRepository,
        ITrainingModuleRepository trainingModuleRepository,
        IUnitOfWork unitOfWork,
        IStoredFileRepository storedFileRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _trainingCompletionRepository = trainingCompletionRepository;
        _trainingModuleRepository = trainingModuleRepository;
        _unitOfWork = unitOfWork;
        _storedFileRepository = storedFileRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(UpdateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _trainingCompletionRepository.GetById(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(DomainErrors.MandatoryTraining.Completion.NotFound(request.Id));
        }

        var module = await _trainingModuleRepository.GetById(entity.TrainingModuleId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.StaffId))
            entity.UpdateStaffMember(request.StaffId);

        if (request.TrainingModuleId is not null)
            entity.UpdateTrainingModule(request.TrainingModuleId);

        if (request.NotRequired)
            entity.MarkNotRequired(module);
        else
            entity.SetCompletedDate(request.CompletedDate);

        if (request.File is null)
        {
            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();

        }

        var existingFile = await _storedFileRepository.GetTrainingCertificateByLinkId(entity.Id.ToString(), cancellationToken);

        if (existingFile is not null)
        {
            existingFile.FileData = request.File.FileData;
            existingFile.FileType = request.File.FileType;
            existingFile.CreatedAt = _dateTimeProvider.Now;
            existingFile.Name = request.File.FileName;
        }
        else
        {
            var fileEntity = new StoredFile
            {
                Name = request.File.FileName,
                FileType = request.File.FileType,
                FileData = request.File.FileData,
                CreatedAt = _dateTimeProvider.Now,
                LinkType = StoredFile.TrainingCertificate,
                LinkId = entity.Id.ToString()
            };

            entity.LinkStoredFile(fileEntity);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
