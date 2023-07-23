namespace Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingCompletionCommandHandler 
    : ICommandHandler<UpdateTrainingCompletionCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger _logger;

    public UpdateTrainingCompletionCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IUnitOfWork unitOfWork,
        IStoredFileRepository storedFileRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger logger)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _unitOfWork = unitOfWork;
        _storedFileRepository = storedFileRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger.ForContext<UpdateTrainingCompletionCommand>();
    }

    public async Task<Result> Handle(UpdateTrainingCompletionCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetById(request.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.TrainingModuleId);

            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.TrainingModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger.Warning("Could not find Training Completion with Id {id}", request.CompletionId);

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

        if (request.File is null)
        {
            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();

        }

        StoredFile existingFile = await _storedFileRepository.GetTrainingCertificateByLinkId(record.Id.ToString(), cancellationToken);

        if (existingFile is not null)
        {
            existingFile.FileData = request.File.FileData;
            existingFile.FileType = request.File.FileType;
            existingFile.CreatedAt = _dateTimeProvider.Now;
            existingFile.Name = request.File.FileName;
        }
        else
        {
            StoredFile fileEntity = new()
            {
                Name = request.File.FileName,
                FileType = request.File.FileType,
                FileData = request.File.FileData,
                CreatedAt = _dateTimeProvider.Now,
                LinkType = StoredFile.TrainingCertificate,
                LinkId = record.Id.ToString()
            };

            record.LinkStoredFile(fileEntity);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
