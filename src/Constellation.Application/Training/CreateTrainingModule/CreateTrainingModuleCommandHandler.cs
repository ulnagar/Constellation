namespace Constellation.Application.Training.CreateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingModuleCommandHandler
    : ICommandHandler<CreateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule entity = TrainingModule.Create(
            request.Name,
            request.Expiry,
            request.Url);

        _trainingRepository.Insert(entity);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
