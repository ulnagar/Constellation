namespace Constellation.Application.MandatoryTraining.CreateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingModuleCommandHandler
    : ICommandHandler<CreateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        var entity = TrainingModule.Create(
            new TrainingModuleId(),
            request.Name,
            request.Expiry,
            request.Url,
            request.CanMarkNotRequired);

        _trainingModuleRepository.Insert(entity);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
